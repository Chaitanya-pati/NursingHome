/**
 * pdfGenerator.js
 *
 * Reliable A4 PDF generation for registration forms.
 *
 * Approach:
 *  - Build a completely self-contained, inline-styled HTML string.
 *  - Inject it into a REAL, fixed-position off-screen <div> appended to
 *    <body>.  Passing a real DOM element (not a string) lets html2canvas
 *    measure the element correctly and avoids the blank-page / clipped-
 *    content bugs that occur when html2pdf creates its own container.
 *  - The off-screen div is styled position:fixed; left:-9999px so it never
 *    affects the visible page.
 *  - After the PDF is saved the temporary div is removed.
 */
(function (window) {
    'use strict';

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
    /*  HTML escaping                                                       */
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
    /*  Section builders — 100 % inline styles, zero Bootstrap            */
    /* ================================================================== */

    /* ── Header row ── */
    function buildHeader(logoPlus, logoSteth, addrLine1, mobNums, email) {
        var imgL = logoPlus
            ? '<img src="' + logoPlus  + '" style="height:62px;width:76px;object-fit:contain;display:block;" />'
            : '';
        var imgR = logoSteth
            ? '<img src="' + logoSteth + '" style="height:62px;width:76px;object-fit:contain;display:block;" />'
            : '';

        return '<tr>' +
            '<td colspan="2" style="padding:10px 14px 8px;border:1px solid #999;">' +
              /* brand */
              '<table style="width:100%;border-collapse:collapse;"><tbody><tr>' +
                '<td style="width:82px;vertical-align:middle;">' + imgL + '</td>' +
                '<td style="text-align:center;vertical-align:middle;padding:0 8px;">' +
                  '<div style="font-family:fantasy,serif;font-size:38px;color:red;' +
                              'letter-spacing:4px;line-height:1.1;margin:0;font-weight:normal;">' +
                    'VEDHANTH' +
                  '</div>' +
                  '<div style="font-size:19px;color:red;font-weight:500;margin-top:3px;">' +
                    'Home Nursing Helpline' +
                  '</div>' +
                '</td>' +
                '<td style="width:82px;vertical-align:middle;text-align:right;">' + imgR + '</td>' +
              '</tr></tbody></table>' +
              /* address */
              '<div style="text-align:center;margin-top:6px;font-size:12px;font-weight:500;">' +
                esc(addrLine1) +
              '</div>' +
              '<div style="text-align:center;font-size:12px;font-weight:500;">' +
                'Mob : ' + esc(mobNums) + ' &nbsp;&nbsp; Email : ' + esc(email) +
              '</div>' +
            '</td></tr>';
    }

    /* ── Form-title row: title left, No / Date right ── */
    function buildTitleRow(formTitle, serialNo, date) {
        return '<tr>' +
            '<td colspan="2" style="padding:8px 14px;border:1px solid #999;">' +
              '<table style="width:100%;border-collapse:collapse;"><tbody><tr>' +
                '<td style="width:56%;text-align:right;padding-right:18px;vertical-align:middle;">' +
                  '<span style="color:red;font-size:14px;font-weight:700;">' + esc(formTitle) + '</span>' +
                '</td>' +
                '<td style="width:44%;vertical-align:middle;">' +
                  '<table style="width:100%;border-collapse:collapse;"><tbody>' +
                    '<tr>' +
                      '<td style="font-size:12px;white-space:nowrap;padding:2px 6px 2px 0;width:44px;">No :</td>' +
                      '<td style="border-bottom:1px solid #666;font-size:12px;padding:2px 4px;">' +
                        esc(serialNo) +
                      '</td>' +
                    '</tr>' +
                    '<tr>' +
                      '<td style="font-size:12px;white-space:nowrap;padding:6px 6px 2px 0;">Date :</td>' +
                      '<td style="border-bottom:1px solid #666;font-size:12px;padding:6px 4px 2px;">' +
                        esc(date) +
                      '</td>' +
                    '</tr>' +
                  '</tbody></table>' +
                '</td>' +
              '</tr></tbody></table>' +
            '</td></tr>';
    }

    /* ── Field rows ── */
    function buildFieldRows(fields) {
        return fields.map(function (f) {
            return '<tr style="page-break-inside:avoid;">' +
                '<td style="width:35%;padding:7px 12px;border:1px solid #999;' +
                           'font-size:12px;font-weight:600;vertical-align:middle;' +
                           'word-wrap:break-word;">' +
                  esc(f.label) +
                '</td>' +
                '<td style="width:65%;padding:7px 12px;border:1px solid #999;' +
                           'font-size:12px;vertical-align:middle;word-wrap:break-word;">' +
                  esc(f.value) +
                '</td>' +
                '</tr>';
        }).join('');
    }

    /* ── Terms + Declaration + Signatures ── */
    function buildTermsRow(termsTitle, terms, declarationText, clientSignSrc, authSignSrc) {

        var termLines = terms.map(function (t, i) {
            return '<div style="display:table;width:100%;margin-top:8px;' +
                              'page-break-inside:avoid;">' +
                '<div style="display:table-cell;width:26px;font-size:12px;' +
                            'font-weight:700;vertical-align:top;text-align:right;' +
                            'padding-right:6px;">' +
                  (i + 1) + '.' +
                '</div>' +
                '<div style="display:table-cell;font-size:12px;line-height:1.55;' +
                            'vertical-align:top;">' +
                  esc(t) +
                '</div>' +
                '</div>';
        }).join('');

        var clientImg = clientSignSrc
            ? '<img src="' + clientSignSrc + '" style="max-width:140px;max-height:58px;' +
              'object-fit:contain;display:block;" />'
            : '<div style="height:58px;"></div>';

        var authImg = authSignSrc
            ? '<img src="' + authSignSrc + '" style="max-width:140px;max-height:58px;' +
              'object-fit:contain;display:block;" />'
            : '<div style="height:58px;"></div>';

        return '<tr><td colspan="2" style="padding:12px 14px 18px;border:1px solid #999;">' +
            /* heading */
            '<div style="font-size:13px;font-weight:700;margin-bottom:4px;">' +
              esc(termsTitle) +
            '</div>' +
            termLines +
            /* declaration */
            '<div style="font-size:13px;font-weight:700;margin-top:14px;margin-bottom:5px;">' +
              'Declaration' +
            '</div>' +
            '<div style="font-size:12px;line-height:1.55;margin-bottom:22px;">' +
              esc(declarationText) +
            '</div>' +
            /* signatures */
            '<table style="width:100%;border-collapse:collapse;"><tbody><tr>' +
              '<td style="width:33%;vertical-align:bottom;padding-bottom:4px;">' +
                clientImg +
                '<div style="border-top:1px solid #444;margin-top:5px;padding-top:3px;' +
                            'font-size:12px;">Signature of the Client</div>' +
              '</td>' +
              '<td style="width:34%;"></td>' +
              '<td style="width:33%;vertical-align:bottom;padding-bottom:4px;text-align:right;">' +
                authImg +
                '<div style="border-top:1px solid #444;margin-top:5px;padding-top:3px;' +
                            'font-size:12px;">Authorised Signatory</div>' +
              '</td>' +
            '</tr></tbody></table>' +
            '</td></tr>';
    }

    /* ================================================================== */
    /*  Public API                                                          */
    /* ================================================================== */

    /**
     * generateRegistrationPDF(config)
     *
     * config = {
     *   filename, formTitle, serialNo, date,
     *   fields [{label, value}],
     *   termsTitle, terms [], declarationText,
     *   clientSignSrc, authorizedSignSrc,
     *   addressLine1, mobileNumbers, email
     * }
     */
    window.generateRegistrationPDF = async function (config) {

        /* 1. Fetch logos as data URIs */
        var base   = window.location.origin;
        var logos  = await Promise.all([
            fetchDataUri(base + '/images/plus%20icon.png'),
            fetchDataUri(base + '/images/stethoscope.png')
        ]);

        /* 2. Build the full table HTML */
        var rows =
            buildHeader(logos[0], logos[1],
                        config.addressLine1, config.mobileNumbers, config.email) +
            buildTitleRow(config.formTitle, config.serialNo, config.date) +
            buildFieldRows(config.fields) +
            buildTermsRow(
                config.termsTitle,
                config.terms,
                config.declarationText,
                config.clientSignSrc     || '',
                config.authorizedSignSrc || ''
            );

        /*
         * 3. A4 at 96 dpi = 794 px wide.
         *    With 0.28 in side margins the content area = (8.27-0.56)*96 ≈ 741 px.
         *    We set the container to exactly 741 px so the table fills the page.
         */
        var CONTENT_W = 741;

        var innerHtml =
            '<table style="width:100%;border-collapse:collapse;border:1px solid #999;' +
                          'table-layout:fixed;">' +
              '<tbody>' + rows + '</tbody>' +
            '</table>';

        /* 4. Create an off-screen real DOM container.
              html2canvas works far more reliably with a real element than with
              the hidden iframe html2pdf creates internally for string input. */
        var container = document.createElement('div');
        container.setAttribute('id', '__pdf_render_container__');
        container.style.cssText = [
            'position:fixed',
            'left:-9999px',
            'top:0',
            'width:' + CONTENT_W + 'px',
            'background:#fff',
            'font-family:Arial,Helvetica,sans-serif',
            'font-size:13px',
            'box-sizing:border-box',
            'z-index:-1',
        ].join(';');
        container.innerHTML = innerHtml;
        document.body.appendChild(container);

        /* 5. Give the browser one frame to lay out the element */
        await new Promise(function (r) { requestAnimationFrame(r); });

        /* 6. Render */
        var opts = {
            margin:      [0.35, 0.28, 0.25, 0.28],
            filename:    config.filename || 'registration.pdf',
            image:       { type: 'jpeg', quality: 0.98 },
            html2canvas: {
                scale:       2,
                useCORS:     true,
                allowTaint:  true,
                logging:     false,
                width:       CONTENT_W,          /* explicit pixel width */
                windowWidth: CONTENT_W,
                x:           0,
                y:           0,
                scrollX:     0,
                scrollY:     0,
            },
            jsPDF:       { unit: 'in', format: 'a4', orientation: 'portrait' },
            pagebreak:   { mode: ['css', 'legacy'], avoid: ['tr', '.sig-block'] },
        };

        try {
            await html2pdf().from(container).set(opts).save();
        } finally {
            /* 7. Always clean up */
            document.body.removeChild(container);
        }
    };

})(window);
