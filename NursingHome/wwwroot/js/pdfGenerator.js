/**
 * pdfGenerator.js
 *
 * Generates a PDF by cloning the actual form DOM so the output is
 * pixel-identical to what the user sees on screen.
 *
 * Strategy:
 *  1. Deep-clone the modal-body (.registrationFormContent).
 *  2. Replace every <input>/<select> with a plain <span> showing its value.
 *  3. Replace signature <canvas> elements with <img> data-URI tags.
 *  4. Remove interactive chrome (file inputs, refresh icons, hidden inputs …).
 *  5. Inject an inline <style> that overrides every Bootstrap responsive rule
 *     so the layout is always rendered at fixed A4 widths regardless of the
 *     device viewport.
 *  6. Wrap everything in a 794 px container and hand it to html2pdf as an
 *     HTML string (most reliable path — avoids blank-page bugs).
 */

(function (window) {
    'use strict';

    /* ------------------------------------------------------------------ */
    /*  CSS override block injected into every PDF render                  */
    /* ------------------------------------------------------------------ */
    var PDF_STYLE = [
        '* { box-sizing: border-box !important; }',

        /* ---- Force all Bootstrap grid columns to their desktop widths ---- */
        '.container, .container-fluid { width: 100% !important; padding: 0 !important; }',
        '.row { display: flex !important; flex-wrap: wrap !important; margin: 0 !important; }',
        '.col-1  { flex: 0 0  8.333% !important; max-width:  8.333% !important; width:  8.333% !important; padding: 0 4px !important; }',
        '.col-2  { flex: 0 0 16.667% !important; max-width: 16.667% !important; width: 16.667% !important; padding: 0 4px !important; }',
        '.col-3  { flex: 0 0 25%     !important; max-width: 25%     !important; width: 25%     !important; padding: 0 4px !important; }',
        '.col-4  { flex: 0 0 33.333% !important; max-width: 33.333% !important; width: 33.333% !important; padding: 0 4px !important; }',
        '.col-5  { flex: 0 0 41.667% !important; max-width: 41.667% !important; width: 41.667% !important; padding: 0 4px !important; }',
        '.col-6  { flex: 0 0 50%     !important; max-width: 50%     !important; width: 50%     !important; padding: 0 4px !important; }',
        '.col-7  { flex: 0 0 58.333% !important; max-width: 58.333% !important; width: 58.333% !important; padding: 0 4px !important; }',
        '.col-8  { flex: 0 0 66.667% !important; max-width: 66.667% !important; width: 66.667% !important; padding: 0 4px !important; }',
        '.col-9  { flex: 0 0 75%     !important; max-width: 75%     !important; width: 75%     !important; padding: 0 4px !important; }',
        '.col-10 { flex: 0 0 83.333% !important; max-width: 83.333% !important; width: 83.333% !important; padding: 0 4px !important; }',
        '.col-11 { flex: 0 0 91.667% !important; max-width: 91.667% !important; width: 91.667% !important; padding: 0 4px !important; }',
        '.col-12 { flex: 0 0 100%    !important; max-width: 100%    !important; width: 100%    !important; padding: 0 4px !important; }',

        /* ---- Tables ---- */
        '.table { width: 100% !important; border-collapse: collapse !important; table-layout: fixed !important; }',
        '.table-bordered td, .table-bordered th { border: 1px solid #aaa !important; padding: 6px 10px !important; vertical-align: middle !important; word-wrap: break-word !important; }',
        'td, th { word-wrap: break-word !important; overflow-wrap: break-word !important; }',

        /* ---- Suppress Bootstrap's mobile table stacking ---- */
        '.modal-body table tr td:first-child { display: table-cell !important; width: auto !important; font-weight: normal !important; padding-bottom: inherit !important; }',
        '.modal-body table tr td:last-child  { display: table-cell !important; width: auto !important; padding-top: inherit !important; }',

        /* ---- Inputs become transparent so only the text value shows ---- */
        'input, select, textarea { border: none !important; background: transparent !important; padding: 0 !important; margin: 0 !important; box-shadow: none !important; -webkit-appearance: none !important; width: 100% !important; font-size: inherit !important; font-family: inherit !important; }',
        '.form-control { border: none !important; background: transparent !important; box-shadow: none !important; padding: 0 !important; }',

        /* ---- Alignment helpers ---- */
        '.contentCenter { display: flex !important; justify-content: center !important; align-items: center !important; }',
        '.contentEnd    { display: flex !important; justify-content: flex-end   !important; align-items: center !important; }',

        /* ---- Signatures always side-by-side ---- */
        '.signature-section { display: flex !important; flex-direction: row !important; align-items: flex-end !important; }',
        '.signature-box     { width: 33.333% !important; flex: 0 0 33.333% !important; }',

        /* ---- Remove all @media responsive overrides (replaced above) ---- */
        /* ---- Prevent page-break inside key sections ---- */
        'table, tr, td { page-break-inside: avoid !important; }',
        '.signature-section { page-break-inside: avoid !important; }',

        /* ---- Hide elements that must not appear in PDF ---- */
        '.pdf-hide { display: none !important; }',

        /* ---- Margins / spacing ---- */
        '.mt-2 { margin-top: 8px !important; }',
        '.mt-1 { margin-top: 4px !important; }',
        '.ml-2 { margin-left: 8px !important; }',
        '.mb-2 { margin-bottom: 8px !important; }',
        '.me-1 { margin-right: 4px !important; }',
    ].join('\n');

    /* ------------------------------------------------------------------ */
    /*  Helpers                                                             */
    /* ------------------------------------------------------------------ */

    function fetchAsDataUri(url) {
        return fetch(url)
            .then(function (r) { return r.blob(); })
            .then(function (blob) {
                return new Promise(function (resolve) {
                    var reader = new FileReader();
                    reader.onload  = function (e) { resolve(e.target.result); };
                    reader.onerror = function ()  { resolve(''); };
                    reader.readAsDataURL(blob);
                });
            })
            .catch(function () { return ''; });
    }

    /**
     * Replace every <img> inside el whose src is a relative /images/… path
     * with a data-URI version so html2canvas never needs a network request.
     */
    function inlineImages(el) {
        var imgs = el.querySelectorAll('img');
        var promises = [];
        imgs.forEach(function (img) {
            var src = img.getAttribute('src') || '';
            if (!src || src.startsWith('data:')) return;
            var absolute = src.startsWith('http') ? src : (window.location.origin + (src.startsWith('/') ? '' : '/') + src);
            promises.push(
                fetchAsDataUri(absolute).then(function (dataUri) {
                    if (dataUri) img.setAttribute('src', dataUri);
                })
            );
        });
        return Promise.all(promises);
    }

    /* ------------------------------------------------------------------ */
    /*  Public API                                                          */
    /* ------------------------------------------------------------------ */

    /**
     * generatePDFFromForm(containerEl, filename, clientSignSrc, authorizedSignSrc)
     *
     * containerEl      – the DOM element wrapping the form (modal-body div)
     * filename         – output file name
     * clientSignSrc    – data URI for client signature (or '')
     * authorizedSignSrc– data URI for authorized signature (or '')
     */
    window.generatePDFFromForm = async function (containerEl, filename, clientSignSrc, authorizedSignSrc) {

        /* 1. Deep-clone so we never mutate the live form */
        var clone = containerEl.cloneNode(true);

        /* 2. Replace <input type="text|number"> with plain spans */
        clone.querySelectorAll('input[type="text"], input[type="number"]').forEach(function (el) {
            var span = document.createElement('span');
            span.style.cssText = 'display:inline-block;width:100%;word-break:break-word;white-space:pre-wrap;';
            span.textContent = el.value || '';
            el.parentNode.replaceChild(span, el);
        });

        /* 3. Replace <input type="date"> with plain spans */
        clone.querySelectorAll('input[type="date"]').forEach(function (el) {
            var span = document.createElement('span');
            span.style.cssText = 'display:inline-block;';
            span.textContent = el.value || '';
            el.parentNode.replaceChild(span, el);
        });

        /* 4. Replace <select> with the selected option text */
        clone.querySelectorAll('select').forEach(function (el) {
            var span = document.createElement('span');
            span.style.cssText = 'display:inline-block;width:100%;';
            span.textContent = el.selectedIndex >= 0 && el.options[el.selectedIndex]
                ? el.options[el.selectedIndex].text : '';
            el.parentNode.replaceChild(span, el);
        });

        /* 5. Remove elements that must not appear in the PDF */
        [
            'input[type="file"]',
            'input[type="hidden"]',
            '#idProofStatus',
            '#downloadIdProof',
            '.refreshSign',
            'button',
        ].forEach(function (sel) {
            clone.querySelectorAll(sel).forEach(function (el) {
                el.parentNode && el.parentNode.removeChild(el);
            });
        });

        /* 6. Replace signature <canvas> elements with <img> or blank spacer */
        clone.querySelectorAll('canvas').forEach(function (canvas) {
            var id = canvas.id || '';
            var src = (id === 'clientSignature') ? clientSignSrc
                    : (id === 'authorizedSignature') ? authorizedSignSrc
                    : '';
            if (src) {
                var img = document.createElement('img');
                img.src = src;
                img.style.cssText = 'max-width:140px;max-height:60px;object-fit:contain;display:block;';
                canvas.parentNode.replaceChild(img, canvas);
            } else {
                var spacer = document.createElement('div');
                spacer.style.height = '60px';
                canvas.parentNode.replaceChild(spacer, canvas);
            }
        });

        /* 7. Hide the signature <img> placeholders (already replaced above via canvas) */
        clone.querySelectorAll('.signatureContainerImage').forEach(function (img) {
            img.style.display = 'none';
        });

        /* 8. Inline all /images/… logo srcs as data URIs */
        await inlineImages(clone);

        /* 9. Build the final printable wrapper */
        var wrapper = document.createElement('div');
        wrapper.style.cssText = [
            'width:754px',
            'font-family:Arial,Helvetica,sans-serif',
            'font-size:13px',
            'background:#fff',
            'padding:16px 20px',
            'box-sizing:border-box',
        ].join(';');

        var styleTag = document.createElement('style');
        styleTag.textContent = PDF_STYLE;

        wrapper.appendChild(styleTag);
        wrapper.appendChild(clone);

        /* 10. Convert to string so html2pdf can use its own container
               (passing a string avoids the blank-page bug caused by
               off-screen DOM elements being clipped by html2canvas) */
        var htmlString = wrapper.outerHTML;

        /* 11. Render */
        var opts = {
            margin:      [0.35, 0.25, 0.25, 0.25],
            filename:    filename || 'registration.pdf',
            image:       { type: 'jpeg', quality: 0.98 },
            html2canvas: {
                scale:       2.5,
                useCORS:     true,
                allowTaint:  true,
                logging:     false,
                windowWidth: 794,
                scrollX:     0,
                scrollY:     0,
            },
            jsPDF:       { unit: 'in', format: 'a4', orientation: 'portrait' },
            pagebreak:   { mode: ['avoid-all', 'css', 'legacy'] },
        };

        await html2pdf().from(htmlString).set(opts).save();
    };

})(window);
