/**
 * pdfGenerator.js
 *
 * Builds a completely self-contained, inline-styled HTML string that
 * replicates the registration form layout exactly — no Bootstrap, no
 * external CSS.  The string is handed directly to html2pdf(), which
 * creates its own properly-positioned container internally.
 *
 * config = {
 *   filename          : string
 *   formTitle         : string          e.g. "Registration Form:"
 *   serialNo          : string
 *   date              : string
 *   fields            : [{label, value}]
 *   termsTitle        : string
 *   terms             : string[]
 *   declarationText   : string
 *   clientSignSrc     : string          data URI or ''
 *   authorizedSignSrc : string          data URI or ''
 *   addressLine1      : string
 *   mobileNumbers     : string
 *   email             : string
 * }
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
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    /* ------------------------------------------------------------------ */
    /*  Section builders (all inline styles)                               */
    /* ------------------------------------------------------------------ */

    /* ── Header: logo | VEDHANTH + subtitle | logo ── */
    function buildHeader(logoPlus, logoSteth, addressLine1, mobileNumbers, email) {
        var imgL = logoPlus  ? '<img src="' + logoPlus  + '" style="height:65px;width:80px;object-fit:contain;display:block;" />' : '';
        var imgR = logoSteth ? '<img src="' + logoSteth + '" style="height:65px;width:80px;object-fit:contain;display:block;margin-left:auto;" />' : '';

        return '<tr>' +
            '<td colspan="2" style="padding:10px 14px 8px;border:1px solid #999;">' +
              /* logos + brand name */
              '<table style="width:100%;border-collapse:collapse;table-layout:fixed;"><tbody><tr>' +
                '<td style="width:88px;vertical-align:middle;">' + imgL + '</td>' +
                '<td style="text-align:center;vertical-align:middle;padding:0 6px;">' +
                  '<div style="font-family:fantasy,serif;font-size:40px;color:red;letter-spacing:4px;line-height:1.1;margin:0;">VEDHANTH</div>' +
                  '<div style="font-size:20px;color:red;font-weight:500;margin-top:2px;">Home Nursing Helpline</div>' +
                '</td>' +
                '<td style="width:88px;vertical-align:middle;">' + imgR + '</td>' +
              '</tr></tbody></table>' +
              /* address */
              '<div style="text-align:center;margin-top:6px;font-size:12px;font-weight:500;">' + esc(addressLine1) + '</div>' +
              '<div style="text-align:center;font-size:12px;font-weight:500;">Mob : ' + esc(mobileNumbers) + ' &nbsp;&nbsp; Email : ' + esc(email) + '</div>' +
            '</td>' +
            '</tr>';
    }

    /* ── Form title row: "Registration Form:" on the left, No / Date on the right ── */
    function buildTitleRow(formTitle, serialNo, date) {
        return '<tr>' +
            '<td colspan="2" style="padding:8px 14px;border:1px solid #999;">' +
              '<table style="width:100%;border-collapse:collapse;table-layout:fixed;"><tbody><tr>' +
                /* left 58%: form title */
                '<td style="width:58%;text-align:right;vertical-align:middle;padding-right:16px;">' +
                  '<span style="color:red;font-size:15px;font-weight:700;">' + esc(formTitle) + '</span>' +
                '</td>' +
                /* right 42%: No + Date */
                '<td style="width:42%;vertical-align:middle;">' +
                  '<table style="width:100%;border-collapse:collapse;"><tbody>' +
                    '<tr>' +
                      '<td style="font-size:13px;white-space:nowrap;padding:2px 6px 2px 0;width:42px;">No :</td>' +
                      '<td style="border-bottom:1px solid #555;font-size:13px;padding:2px 4px;">' + esc(serialNo) + '</td>' +
                    '</tr>' +
                    '<tr>' +
                      '<td style="font-size:13px;white-space:nowrap;padding:6px 6px 2px 0;">Date :</td>' +
                      '<td style="border-bottom:1px solid #555;font-size:13px;padding:6px 4px 2px;">' + esc(date) + '</td>' +
                    '</tr>' +
                  '</tbody></table>' +
                '</td>' +
              '</tr></tbody></table>' +
            '</td>' +
            '</tr>';
    }

    /* ── Field rows: label (35%) | value (65%) ── */
    function buildFieldRows(fields) {
        return fields.map(function (f) {
            return '<tr>' +
                '<td style="width:35%;padding:7px 12px;border:1px solid #999;font-size:13px;font-weight:600;vertical-align:middle;word-wrap:break-word;">' +
                  esc(f.label) +
                '</td>' +
                '<td style="width:65%;padding:7px 12px;border:1px solid #999;font-size:13px;vertical-align:middle;word-wrap:break-word;">' +
                  esc(f.value) +
                '</td>' +
                '</tr>';
        }).join('');
    }

    /* ── Terms + Declaration + Signatures ── */
    function buildTermsRow(termsTitle, terms, declarationText, clientSignSrc, authSignSrc) {

        /* numbered term items */
        var termLines = terms.map(function (t, i) {
            return '<div style="display:table;width:100%;margin-top:8px;">' +
                '<div style="display:table-cell;width:28px;font-size:12px;font-weight:700;vertical-align:top;text-align:center;">' + (i + 1) + '.</div>' +
                '<div style="display:table-cell;font-size:12px;line-height:1.6;vertical-align:top;">' + esc(t) + '</div>' +
                '</div>';
        }).join('');

        /* signature images or blank spacers */
        var clientImg = clientSignSrc
            ? '<img src="' + clientSignSrc + '" style="max-width:140px;max-height:60px;object-fit:contain;display:block;" />'
            : '<div style="height:60px;"></div>';
        var authImg = authSignSrc
            ? '<img src="' + authSignSrc + '" style="max-width:140px;max-height:60px;object-fit:contain;display:block;margin-left:auto;" />'
            : '<div style="height:60px;"></div>';

        return '<tr>' +
            '<td colspan="2" style="padding:12px 14px 16px;border:1px solid #999;">' +
              /* terms heading */
              '<div style="font-size:13px;font-weight:700;margin-bottom:4px;">' + esc(termsTitle) + '</div>' +
              termLines +
              /* declaration */
              '<div style="font-size:13px;font-weight:700;margin-top:14px;margin-bottom:6px;">Declaration</div>' +
              '<div style="font-size:12px;line-height:1.6;margin-bottom:22px;">' + esc(declarationText) + '</div>' +
              /* signatures */
              '<table style="width:100%;border-collapse:collapse;table-layout:fixed;"><tbody><tr>' +
                '<td style="width:33%;vertical-align:bottom;padding-bottom:4px;">' +
                  clientImg +
                  '<div style="border-top:1px solid #444;margin-top:6px;padding-top:4px;font-size:12px;">Signature of the Client</div>' +
                '</td>' +
                '<td style="width:34%;"></td>' +
                '<td style="width:33%;vertical-align:bottom;padding-bottom:4px;">' +
                  authImg +
                  '<div style="border-top:1px solid #444;margin-top:6px;padding-top:4px;font-size:12px;text-align:right;">Authorised Signatory</div>' +
                '</td>' +
              '</tr></tbody></table>' +
            '</td>' +
            '</tr>';
    }

    /* ------------------------------------------------------------------ */
    /*  Public API                                                          */
    /* ------------------------------------------------------------------ */
    window.generateRegistrationPDF = async function (config) {

        /* Fetch logos as data URIs so html2canvas never makes a network call */
        var base = window.location.origin;
        var logos = await Promise.all([
            fetchDataUri(base + '/images/plus%20icon.png'),
            fetchDataUri(base + '/images/stethoscope.png')
        ]);

        /* Build the complete inline-styled table */
        var rows =
            buildHeader(logos[0], logos[1], config.addressLine1, config.mobileNumbers, config.email) +
            buildTitleRow(config.formTitle, config.serialNo, config.date) +
            buildFieldRows(config.fields) +
            buildTermsRow(
                config.termsTitle,
                config.terms,
                config.declarationText,
                config.clientSignSrc    || '',
                config.authorizedSignSrc || ''
            );

        /*
         * Outer wrapper: fixed 740 px so it fits inside A4 content area.
         * At windowWidth 794 and margin 0.25 in × 2 the content area
         * on the PDF page is ≈ 7.77 in = 745 px — 740 px is safe.
         */
        var html =
            '<div style="' +
              'width:740px;' +
              'font-family:Arial,Helvetica,sans-serif;' +
              'background:#fff;' +
              'padding:0;' +
              'box-sizing:border-box;' +
            '">' +
              '<table style="' +
                'width:100%;' +
                'border-collapse:collapse;' +
                'border:1px solid #999;' +
                'table-layout:fixed;' +
                'page-break-inside:auto;' +
              '">' +
                '<tbody>' + rows + '</tbody>' +
              '</table>' +
            '</div>';

        var opts = {
            margin:      [0.35, 0.28, 0.25, 0.28],   /* top right bottom left (inches) */
            filename:    config.filename || 'registration.pdf',
            image:       { type: 'jpeg', quality: 0.98 },
            html2canvas: {
                scale:       2,
                useCORS:     true,
                allowTaint:  true,
                logging:     false,
                windowWidth: 794,          /* A4 pixel width at 96 dpi */
                scrollX:     0,
                scrollY:     0,
            },
            jsPDF:       { unit: 'in', format: 'a4', orientation: 'portrait' },
            pagebreak:   { mode: ['css', 'legacy'], avoid: ['tr', 'td', '.signature-block'] },
        };

        await html2pdf().from(html).set(opts).save();
    };

})(window);
