/**
 * pdfGenerator.js
 *
 * Uses jsPDF + html2canvas directly (both are available via the
 * html2pdf bundle already loaded on the page).
 *
 * Strategy that avoids every known blank/clipped-page issue:
 *  1. Build a fully inline-styled HTML string (no Bootstrap dependency).
 *  2. Inject it into a real <div> at position:fixed, left:0, top:0,
 *     width:741px, z-index:-1  — it sits at the document origin so
 *     html2canvas captures it at (0,0), and the negative z-index keeps
 *     it behind the modal overlay.
 *  3. Use html2canvas directly (not via html2pdf wrapper) so we control
 *     every option.
 *  4. Manually slice the resulting tall canvas into A4 pages with jsPDF.
 *  5. Remove the temporary element in a finally block.
 *
 * A4 maths:
 *   A4 = 210 mm × 297 mm.
 *   At 96 dpi: 1 px = 25.4/96 = 0.26458 mm.
 *   Side margins 7 mm → content width = 196 mm = 741 px  ✓
 *   Top/bottom margins 8 mm → content height = 281 mm.
 */
(function (window) {
    'use strict';

    /* ------------------------------------------------------------------ */
    /*  Constants (keep in sync with jsPDF call below)                     */
    /* ------------------------------------------------------------------ */
    var CONTENT_PX  = 741;      // element width in CSS px → fits A4 exactly
    var MARGIN_MM   = 7;        // side margins in mm
    var MARGIN_T_MM = 8;        // top/bottom margin in mm
    var A4_W_MM     = 210;
    var A4_H_MM     = 297;
    var CONTENT_W_MM = A4_W_MM - MARGIN_MM * 2;       // 196 mm
    var CONTENT_H_MM = A4_H_MM - MARGIN_T_MM * 2;     // 281 mm

    /* ------------------------------------------------------------------ */
    /*  Logo fetcher                                                        */
    /* ------------------------------------------------------------------ */
    function fetchDataUri(url) {
        return fetch(url)
            .then(function (r) { return r.blob(); })
            .then(function (blob) {
                return new Promise(function (resolve) {
                    var fr = new FileReader();
                    fr.onload  = function (e) { resolve(e.target.result); };
                    fr.onerror = function ()  { resolve(''); };
                    fr.readAsDataURL(blob);
                });
            })
            .catch(function () { return ''; });
    }

    /* ------------------------------------------------------------------ */
    /*  HTML escape                                                         */
    /* ------------------------------------------------------------------ */
    function esc(s) {
        if (s == null) return '';
        return String(s)
            .replace(/&/g, '&amp;')
            .replace(/</g,  '&lt;')
            .replace(/>/g,  '&gt;')
            .replace(/"/g,  '&quot;');
    }

    /* ================================================================== */
    /*  Section builders — 100 % inline styles                            */
    /* ================================================================== */

    function buildHeader(imgL, imgR, addr1, mob, email) {
        var l = imgL ? '<img src="' + imgL + '" style="height:60px;width:74px;object-fit:contain;display:block;">' : '';
        var r = imgR ? '<img src="' + imgR + '" style="height:60px;width:74px;object-fit:contain;display:block;">' : '';
        return '<tr><td colspan="2" style="padding:10px 14px 8px;border:1px solid #999;">' +
            '<table style="width:100%;border-collapse:collapse;"><tr>' +
              '<td style="width:80px;vertical-align:middle;">' + l + '</td>' +
              '<td style="text-align:center;vertical-align:middle;padding:0 6px;">' +
                '<div style="font-family:fantasy,serif;font-size:36px;color:red;letter-spacing:3px;line-height:1.1;">VEDHANTH</div>' +
                '<div style="font-size:18px;color:red;font-weight:500;margin-top:2px;">Home Nursing Helpline</div>' +
              '</td>' +
              '<td style="width:80px;vertical-align:middle;text-align:right;">' + r + '</td>' +
            '</tr></table>' +
            '<div style="text-align:center;margin-top:5px;font-size:11.5px;font-weight:500;">' + esc(addr1) + '</div>' +
            '<div style="text-align:center;font-size:11.5px;font-weight:500;">Mob : ' + esc(mob) + ' &nbsp; Email : ' + esc(email) + '</div>' +
        '</td></tr>';
    }

    function buildTitleRow(title, no, date) {
        return '<tr><td colspan="2" style="padding:7px 14px;border:1px solid #999;">' +
            '<table style="width:100%;border-collapse:collapse;"><tr>' +
              '<td style="width:56%;text-align:right;padding-right:16px;vertical-align:middle;">' +
                '<span style="color:red;font-size:13.5px;font-weight:700;">' + esc(title) + '</span>' +
              '</td>' +
              '<td style="width:44%;vertical-align:middle;">' +
                '<table style="width:100%;border-collapse:collapse;">' +
                  '<tr>' +
                    '<td style="font-size:12px;white-space:nowrap;padding:2px 5px 2px 0;width:42px;">No :</td>' +
                    '<td style="border-bottom:1px solid #666;font-size:12px;padding:2px 4px;">' + esc(no) + '</td>' +
                  '</tr>' +
                  '<tr>' +
                    '<td style="font-size:12px;white-space:nowrap;padding:5px 5px 2px 0;">Date :</td>' +
                    '<td style="border-bottom:1px solid #666;font-size:12px;padding:5px 4px 2px;">' + esc(date) + '</td>' +
                  '</tr>' +
                '</table>' +
              '</td>' +
            '</tr></table>' +
        '</td></tr>';
    }

    function buildFields(fields) {
        return fields.map(function (f) {
            return '<tr>' +
                '<td style="width:35%;padding:6px 11px;border:1px solid #999;font-size:12px;font-weight:600;vertical-align:middle;word-break:break-word;">' + esc(f.label) + '</td>' +
                '<td style="width:65%;padding:6px 11px;border:1px solid #999;font-size:12px;vertical-align:middle;word-break:break-word;">' + esc(f.value) + '</td>' +
            '</tr>';
        }).join('');
    }

    function buildTerms(termsTitle, terms, decl, cSign, aSign) {
        var items = terms.map(function (t, i) {
            return '<div style="display:table;width:100%;margin-top:7px;">' +
                '<div style="display:table-cell;width:24px;font-size:11.5px;font-weight:700;vertical-align:top;text-align:right;padding-right:5px;">' + (i + 1) + '.</div>' +
                '<div style="display:table-cell;font-size:11.5px;line-height:1.5;vertical-align:top;">' + esc(t) + '</div>' +
            '</div>';
        }).join('');

        var cImg = cSign ? '<img src="' + cSign + '" style="max-width:130px;max-height:55px;object-fit:contain;display:block;">' : '<div style="height:55px;"></div>';
        var aImg = aSign ? '<img src="' + aSign + '" style="max-width:130px;max-height:55px;object-fit:contain;display:block;">' : '<div style="height:55px;"></div>';

        return '<tr><td colspan="2" style="padding:11px 14px 16px;border:1px solid #999;">' +
            '<div style="font-size:12.5px;font-weight:700;margin-bottom:3px;">' + esc(termsTitle) + '</div>' +
            items +
            '<div style="font-size:12.5px;font-weight:700;margin-top:13px;margin-bottom:4px;">Declaration</div>' +
            '<div style="font-size:11.5px;line-height:1.5;margin-bottom:20px;">' + esc(decl) + '</div>' +
            '<table style="width:100%;border-collapse:collapse;"><tr>' +
              '<td style="width:33%;vertical-align:bottom;">' +
                cImg +
                '<div style="border-top:1px solid #444;margin-top:5px;padding-top:3px;font-size:11.5px;">Signature of the Client</div>' +
              '</td>' +
              '<td style="width:34%;"></td>' +
              '<td style="width:33%;vertical-align:bottom;text-align:right;">' +
                aImg +
                '<div style="border-top:1px solid #444;margin-top:5px;padding-top:3px;font-size:11.5px;">Authorised Signatory</div>' +
              '</td>' +
            '</tr></table>' +
        '</td></tr>';
    }

    /* ================================================================== */
    /*  Public API — capture any live DOM element into an A4 PDF          */
    /* ================================================================== */

    /**
     * syncLiveValues(original, clone)
     *
     * cloneNode(true) copies DOM attributes but NOT JavaScript-set properties.
     * This function patches the clone so that:
     *   • input / textarea / select  show the same typed/set value
     *   • img elements              show the same src (base64 previews, etc.)
     *   • canvas elements           show the same drawn content (signatures)
     */
    function syncLiveValues(original, clone) {
        /* ── inputs / textareas / selects ── */
        var oFields = original.querySelectorAll('input, textarea, select');
        var cFields = clone.querySelectorAll('input, textarea, select');
        oFields.forEach(function (el, i) {
            if (!cFields[i]) return;
            cFields[i].value   = el.value;
            cFields[i].checked = el.checked;
        });

        /* ── images (candidate photo, signature image) ── */
        var oImgs = original.querySelectorAll('img');
        var cImgs = clone.querySelectorAll('img');
        oImgs.forEach(function (el, i) {
            if (cImgs[i]) cImgs[i].src = el.src;
        });

        /* ── canvases (drawn signature pad) ── */
        var oCanvases = original.querySelectorAll('canvas');
        var cCanvases = clone.querySelectorAll('canvas');
        oCanvases.forEach(function (origC, i) {
            if (!cCanvases[i]) return;
            var dCtx = cCanvases[i].getContext('2d');
            cCanvases[i].width  = origC.width;
            cCanvases[i].height = origC.height;
            dCtx.drawImage(origC, 0, 0);
        });
    }

    /**
     * generatePDFFromElement(sourceEl, filename)
     *
     * Strategy:
     *  1. Clone the source element so the full content height is available
     *     (the original may be inside a scrollable modal-body).
     *  2. Sync all live form values, images and canvas drawings from the
     *     original element to the clone (cloneNode doesn't do this).
     *  3. Inject the clone at position:fixed, width = CONTENT_PX (741 px)
     *     so html2canvas renders it at exactly the A4 content width.
     *     windowWidth is set to the real viewport width so Bootstrap's
     *     col-* percentage classes compute the same ratios as on screen.
     *  4. Slice the resulting canvas into A4 pages with jsPDF.
     */
    window.generatePDFFromElement = async function (sourceEl, filename) {

        var clone = sourceEl.cloneNode(true);
        clone.id = '__pdf_el_clone__';
        clone.style.cssText = [
            'position:fixed',
            'left:0',
            'top:0',
            'width:' + CONTENT_PX + 'px',
            'background:#ffffff',
            'box-sizing:border-box',
            'z-index:-1',
            'overflow:visible',
        ].join(';');

        /* Sync live values BEFORE appending so img.src loads correctly */
        syncLiveValues(sourceEl, clone);

        document.body.appendChild(clone);

        /* Two rAF ticks — let the browser fully lay out the clone */
        await new Promise(function (r) { requestAnimationFrame(function () { requestAnimationFrame(r); }); });

        try {
            var canvas = await html2canvas(clone, {
                scale:       2,
                useCORS:     true,
                allowTaint:  true,
                logging:     false,
                width:       CONTENT_PX,
                /* Keep the real viewport width so Bootstrap col-* ratios
                   remain identical to what the user sees on screen        */
                windowWidth: window.innerWidth || document.documentElement.clientWidth,
            });

            var jsPDFCtor = (window.jspdf && window.jspdf.jsPDF) || window.jsPDF;
            var doc = new jsPDFCtor({ unit: 'mm', format: 'a4', orientation: 'portrait' });

            var cW       = canvas.width;          // CONTENT_PX * 2  (scale 2)
            var cH       = canvas.height;
            var mmPerCpx = CONTENT_W_MM / cW;     // exact A4 ratio
            var pageHpx  = Math.floor(CONTENT_H_MM / mmPerCpx);
            var total    = Math.ceil(cH / pageHpx);

            for (var page = 0; page < total; page++) {
                if (page > 0) doc.addPage();
                var srcY  = page * pageHpx;
                var srcH  = Math.min(pageHpx, cH - srcY);
                var destH = srcH * mmPerCpx;
                var strip = document.createElement('canvas');
                strip.width  = cW;
                strip.height = srcH;
                strip.getContext('2d').drawImage(canvas, 0, srcY, cW, srcH, 0, 0, cW, srcH);
                doc.addImage(
                    strip.toDataURL('image/jpeg', 0.97),
                    'JPEG',
                    MARGIN_MM, MARGIN_T_MM,
                    CONTENT_W_MM, destH
                );
            }

            doc.save(filename || 'document.pdf');
        } finally {
            if (clone.parentNode) clone.parentNode.removeChild(clone);
        }
    };

    /* ================================================================== */
    /*  Public API — build PDF from a config object                        */
    /* ================================================================== */
    window.generateRegistrationPDF = async function (config) {

        /* ── 1. Logos as data URIs ── */
        var base  = window.location.origin;
        var logos = await Promise.all([
            fetchDataUri(base + '/images/plus%20icon.png'),
            fetchDataUri(base + '/images/stethoscope.png')
        ]);

        /* ── 2. Build table HTML ── */
        var tableHtml =
            '<table style="width:100%;border-collapse:collapse;border:1px solid #999;table-layout:fixed;">' +
              '<tbody>' +
                buildHeader(logos[0], logos[1], config.addressLine1, config.mobileNumbers, config.email) +
                buildTitleRow(config.formTitle, config.serialNo, config.date) +
                buildFields(config.fields) +
                buildTerms(config.termsTitle, config.terms, config.declarationText,
                           config.clientSignSrc || '', config.authorizedSignSrc || '') +
              '</tbody>' +
            '</table>';

        /* ── 3. Inject real DOM element at (0,0) behind the modal overlay ── */
        var el = document.createElement('div');
        el.id = '__pdf_tmp__';
        el.style.cssText = [
            'position:fixed',
            'left:0',
            'top:0',
            'width:' + CONTENT_PX + 'px',
            'background:#ffffff',
            'font-family:Arial,Helvetica,sans-serif',
            'box-sizing:border-box',
            'z-index:-1',         /* behind modal but in the stacking context */
            'overflow:visible',
        ].join(';');
        el.innerHTML = tableHtml;
        document.body.appendChild(el);

        /* ── 4. Two animation frames so the browser fully lays out the element ── */
        await new Promise(function (r) { requestAnimationFrame(function () { requestAnimationFrame(r); }); });

        try {
            /* ── 5. Render to canvas with html2canvas ── */
            var canvas = await html2canvas(el, {
                scale:       2,              /* 2× for sharp text */
                useCORS:     true,
                allowTaint:  true,
                logging:     false,
                width:       CONTENT_PX,
                windowWidth: CONTENT_PX,
                /* do NOT set x/y/scrollX/scrollY — let html2canvas measure the element */
            });

            /* ── 6. Paginate canvas into A4 jsPDF ── */
            var jsPDFCtor = (window.jspdf && window.jspdf.jsPDF) || window.jsPDF;
            var doc = new jsPDFCtor({
                unit:        'mm',
                format:      'a4',
                orientation: 'portrait',
            });

            /*
             * Pixel ↔ mm conversion:
             *   canvas was rendered at scale 2, so 1 logical px = 2 canvas px.
             *   1 logical px = 0.26458 mm  →  1 canvas px = 0.13229 mm
             *   Content width:  CONTENT_W_MM mm = CONTENT_PX px = CONTENT_PX*2 canvas px
             *   Ratio: CONTENT_W_MM / (CONTENT_PX * 2) mm per canvas px
             */
            var cW         = canvas.width;                            // canvas pixels wide
            var cH         = canvas.height;                           // canvas pixels tall
            var mmPerCpx   = CONTENT_W_MM / cW;                      // mm per canvas pixel
            var pageHpx    = Math.floor(CONTENT_H_MM / mmPerCpx);    // canvas pixels per A4 page
            var totalPages = Math.ceil(cH / pageHpx);

            for (var page = 0; page < totalPages; page++) {
                if (page > 0) doc.addPage();

                var srcY   = page * pageHpx;
                var srcH   = Math.min(pageHpx, cH - srcY);
                var destH  = srcH * mmPerCpx;                        /* mm height on this page */

                /* slice this page's strip out of the full canvas */
                var strip  = document.createElement('canvas');
                strip.width  = cW;
                strip.height = srcH;
                strip.getContext('2d').drawImage(canvas, 0, srcY, cW, srcH, 0, 0, cW, srcH);

                doc.addImage(
                    strip.toDataURL('image/jpeg', 0.97),
                    'JPEG',
                    MARGIN_MM,    /* x */
                    MARGIN_T_MM,  /* y */
                    CONTENT_W_MM, /* width  in mm */
                    destH         /* height in mm */
                );
            }

            doc.save(config.filename || 'registration.pdf');

        } finally {
            /* ── 7. Always remove the temporary element ── */
            if (el.parentNode) el.parentNode.removeChild(el);
        }
    };

})(window);
