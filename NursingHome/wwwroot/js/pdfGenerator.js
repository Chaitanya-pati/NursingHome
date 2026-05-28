/**
 * pdfGenerator.js
 * Shared, device-independent A4 PDF generation for registration forms.
 *
 * Key design:
 *  - Logos are fetched once and converted to data URIs so html2canvas
 *    never has to make a network request during render.
 *  - Signatures are already data URIs (canvas.toDataURL or stored base64).
 *  - The final HTML string (all inline styles, no Bootstrap) is passed
 *    directly to html2pdf() as a string — html2pdf creates its own
 *    correctly-positioned container, which avoids the blank-page bug
 *    that occurs when passing an off-screen DOM element.
 */

(function (window) {
    'use strict';

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

    /** Escape text so it can safely be embedded in HTML attribute / content */
    function esc(str) {
        if (str == null) return '';
        return String(str)
            .replace(/&/g,  '&amp;')
            .replace(/</g,  '&lt;')
            .replace(/>/g,  '&gt;')
            .replace(/"/g,  '&quot;');
    }

    /* ------------------------------------------------------------------ */
    /*  HTML section builders (all inline styles — zero Bootstrap)         */
    /* ------------------------------------------------------------------ */

    function buildHeader(logoPlus, logoSteth, addressLine1, mobileNumbers, email) {
        var plusImg  = logoPlus  ? '<img src="' + logoPlus  + '" style="height:62px;width:78px;object-fit:contain;display:block;" />' : '';
        var stethImg = logoSteth ? '<img src="' + logoSteth + '" style="height:62px;width:78px;object-fit:contain;display:block;margin-left:auto;" />' : '';

        return '<tr>' +
            '<td colspan="2" style="padding:10px 12px 6px;border:1px solid #aaa;">' +
              '<table style="width:100%;border-collapse:collapse;table-layout:fixed;">' +
                '<tbody><tr>' +
                  '<td style="width:90px;vertical-align:middle;">' + plusImg + '</td>' +
                  '<td style="text-align:center;vertical-align:middle;padding:0 8px;">' +
                    '<div style="font-family:fantasy,serif;font-size:42px;color:red;letter-spacing:4px;line-height:1.1;">VEDHANTH</div>' +
                    '<div style="font-size:21px;color:red;font-weight:500;margin-top:-2px;">Home Nursing Helpline</div>' +
                  '</td>' +
                  '<td style="width:90px;vertical-align:middle;">' + stethImg + '</td>' +
                '</tr></tbody>' +
              '</table>' +
              '<div style="text-align:center;margin-top:5px;">' +
                '<div style="font-size:12px;font-weight:500;">' + esc(addressLine1) + '</div>' +
                '<div style="font-size:12px;font-weight:500;">Mob : ' + esc(mobileNumbers) + ' &nbsp; Email : ' + esc(email) + '</div>' +
              '</div>' +
            '</td>' +
            '</tr>';
    }

    function buildTitleRow(formTitle, serialNo, date) {
        return '<tr>' +
            '<td colspan="2" style="padding:8px 12px;border:1px solid #aaa;">' +
              '<table style="width:100%;border-collapse:collapse;table-layout:fixed;">' +
                '<tbody><tr>' +
                  '<td style="width:58%;text-align:right;padding-right:14px;vertical-align:middle;">' +
                    '<span style="color:red;font-size:15px;font-weight:700;">' + esc(formTitle) + '</span>' +
                  '</td>' +
                  '<td style="width:42%;vertical-align:middle;">' +
                    '<table style="width:100%;border-collapse:collapse;">' +
                      '<tbody>' +
                        '<tr>' +
                          '<td style="font-size:13px;white-space:nowrap;padding:2px 4px 2px 0;">No :</td>' +
                          '<td style="border-bottom:1px solid #666;font-size:13px;padding:2px 4px;">' + esc(serialNo) + '</td>' +
                        '</tr>' +
                        '<tr>' +
                          '<td style="font-size:13px;white-space:nowrap;padding:5px 4px 2px 0;">Date :</td>' +
                          '<td style="border-bottom:1px solid #666;font-size:13px;padding:5px 4px 2px;">' + esc(date) + '</td>' +
                        '</tr>' +
                      '</tbody>' +
                    '</table>' +
                  '</td>' +
                '</tr></tbody>' +
              '</table>' +
            '</td>' +
            '</tr>';
    }

    function buildFieldRows(fields) {
        return fields.map(function (f) {
            return '<tr>' +
                '<td style="width:35%;padding:7px 10px;border:1px solid #aaa;font-size:13px;font-weight:600;word-wrap:break-word;vertical-align:top;">' +
                    esc(f.label) +
                '</td>' +
                '<td style="width:65%;padding:7px 10px;border:1px solid #aaa;font-size:13px;word-wrap:break-word;vertical-align:top;">' +
                    esc(f.value) +
                '</td>' +
                '</tr>';
        }).join('');
    }

    function buildTermsAndSignatureRow(termsTitle, terms, declarationText, clientSignSrc, authorizedSignSrc) {
        var termItems = terms.map(function (t, i) {
            return '<div style="display:table;width:100%;margin-top:7px;">' +
                '<div style="display:table-cell;width:30px;font-weight:700;font-size:12px;text-align:center;vertical-align:top;padding-top:1px;">' + (i + 1) + '.</div>' +
                '<div style="display:table-cell;font-size:12px;line-height:1.55;vertical-align:top;">' + esc(t) + '</div>' +
                '</div>';
        }).join('');

        var clientSignHTML = clientSignSrc
            ? '<img src="' + clientSignSrc + '" style="max-width:130px;max-height:55px;object-fit:contain;display:block;" />'
            : '<div style="height:55px;"></div>';

        var authSignHTML = authorizedSignSrc
            ? '<img src="' + authorizedSignSrc + '" style="max-width:130px;max-height:55px;object-fit:contain;display:block;margin-left:auto;" />'
            : '<div style="height:55px;"></div>';

        return '<tr>' +
            '<td colspan="2" style="padding:12px 12px 14px;border:1px solid #aaa;">' +
              '<div style="font-weight:700;font-size:13px;margin-bottom:2px;">' + esc(termsTitle) + '</div>' +
              termItems +
              '<div style="font-weight:700;font-size:13px;margin-top:14px;margin-bottom:5px;">Declaration</div>' +
              '<div style="font-size:12px;line-height:1.55;margin-bottom:20px;">' + esc(declarationText) + '</div>' +
              '<table style="width:100%;border-collapse:collapse;table-layout:fixed;">' +
                '<tbody><tr>' +
                  '<td style="width:33%;vertical-align:bottom;padding-bottom:2px;">' +
                    clientSignHTML +
                    '<div style="border-top:1px solid #555;margin-top:5px;padding-top:3px;font-size:12px;">Signature of the Client</div>' +
                  '</td>' +
                  '<td style="width:34%;"></td>' +
                  '<td style="width:33%;vertical-align:bottom;padding-bottom:2px;">' +
                    authSignHTML +
                    '<div style="border-top:1px solid #555;margin-top:5px;padding-top:3px;font-size:12px;">Authorised Signatory</div>' +
                  '</td>' +
                '</tr></tbody>' +
              '</table>' +
            '</td>' +
            '</tr>';
    }

    /* ------------------------------------------------------------------ */
    /*  Public API                                                          */
    /* ------------------------------------------------------------------ */

    /**
     * generateRegistrationPDF(config)
     *
     * config = {
     *   filename          : string
     *   formTitle         : string       e.g. "OLD AGE ADMISSION FORM:"
     *   serialNo          : string
     *   date              : string       e.g. "2025-07-01"
     *   fields            : [{label, value}]
     *   termsTitle        : string
     *   terms             : string[]     numbered automatically
     *   declarationText   : string
     *   clientSignSrc     : string       data URI or ''
     *   authorizedSignSrc : string       data URI or ''
     *   addressLine1      : string
     *   mobileNumbers     : string
     *   email             : string
     * }
     */
    window.generateRegistrationPDF = async function (config) {
        var baseUrl = window.location.origin;

        /* Fetch logos as data URIs — avoids any CORS/network issues during render */
        var logoResults = await Promise.all([
            fetchAsDataUri(baseUrl + '/images/plus%20icon.png'),
            fetchAsDataUri(baseUrl + '/images/stethoscope.png')
        ]);
        var logoPlus  = logoResults[0];
        var logoSteth = logoResults[1];

        /* Build complete A4 HTML string — only inline styles, no Bootstrap */
        var tableRows =
            buildHeader(logoPlus, logoSteth, config.addressLine1, config.mobileNumbers, config.email) +
            buildTitleRow(config.formTitle, config.serialNo, config.date) +
            buildFieldRows(config.fields) +
            buildTermsAndSignatureRow(
                config.termsTitle,
                config.terms,
                config.declarationText,
                config.clientSignSrc    || '',
                config.authorizedSignSrc || ''
            );

        var htmlString =
            '<div style="' +
              'width:754px;' +
              'font-family:Arial,Helvetica,sans-serif;' +
              'background:#fff;' +
              'padding:20px;' +
              'box-sizing:border-box;' +
            '">' +
              '<table style="' +
                'width:100%;' +
                'border-collapse:collapse;' +
                'border:1px solid #aaa;' +
                'table-layout:fixed;' +
              '">' +
                '<tbody>' + tableRows + '</tbody>' +
              '</table>' +
            '</div>';

        /* ----------------------------------------------------------------
         * Pass as an HTML **string** — html2pdf creates its own properly
         * positioned container internally.  This is the only reliable way
         * to avoid blank-page output; passing an off-screen DOM element
         * causes html2canvas to capture an empty region.
         * ---------------------------------------------------------------- */
        var options = {
            margin:     [0.3, 0.25, 0.15, 0.25],
            filename:   config.filename || 'registration.pdf',
            image:      { type: 'jpeg', quality: 0.98 },
            html2canvas: {
                scale:       2,
                useCORS:     true,
                allowTaint:  true,
                logging:     false,
                windowWidth: 794
            },
            jsPDF:      { unit: 'in', format: 'letter', orientation: 'portrait' },
            pagebreak:  { mode: ['avoid-all', 'css', 'legacy'] }
        };

        await html2pdf().from(htmlString).set(options).save();
    };

})(window);
