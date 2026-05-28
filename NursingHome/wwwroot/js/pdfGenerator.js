/**
 * pdfGenerator.js
 * Shared, device-independent A4 PDF generation for registration forms.
 * Builds a completely isolated HTML structure with only inline styles —
 * zero Bootstrap, zero responsive CSS — so output is identical on
 * mobile, tablet, and desktop.
 */

(function (window) {
    'use strict';

    /* ------------------------------------------------------------------ */
    /*  Internal helpers                                                    */
    /* ------------------------------------------------------------------ */

    function fetchAsDataUri(url) {
        return fetch(url)
            .then(function (r) { return r.blob(); })
            .then(function (blob) {
                return new Promise(function (resolve) {
                    var reader = new FileReader();
                    reader.onload = function (e) { resolve(e.target.result); };
                    reader.onerror = function () { resolve(''); };
                    reader.readAsDataURL(blob);
                });
            })
            .catch(function () { return ''; });
    }

    function waitForImages(container) {
        var imgs = Array.from(container.querySelectorAll('img'));
        return Promise.all(imgs.map(function (img) {
            if (img.complete && img.naturalWidth > 0) return Promise.resolve();
            return new Promise(function (resolve) {
                img.onload = resolve;
                img.onerror = resolve;
            });
        }));
    }

    function esc(str) {
        if (!str) return '';
        return String(str)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }

    /* ------------------------------------------------------------------ */
    /*  HTML builders                                                       */
    /* ------------------------------------------------------------------ */

    function buildHeader(logoPlus, logoSteth, addressLine1, mobileNumbers, email) {
        var plusImg  = logoPlus  ? '<img src="' + logoPlus  + '" style="height:65px;width:80px;object-fit:contain;" />' : '';
        var stethImg = logoSteth ? '<img src="' + logoSteth + '" style="height:65px;width:80px;object-fit:contain;" />' : '';

        return [
            '<tr>',
            '  <td colspan="2" style="padding:10px 12px 6px;border:1px solid #aaa;">',
            '    <table style="width:100%;border-collapse:collapse;table-layout:fixed;">',
            '      <tbody><tr>',
            '        <td style="width:90px;vertical-align:middle;">' + plusImg + '</td>',
            '        <td style="text-align:center;vertical-align:middle;">',
            '          <div style="font-family:fantasy,serif;font-size:44px;color:red;letter-spacing:4px;line-height:1.1;">VEDHANTH</div>',
            '          <div style="font-size:22px;color:red;font-weight:500;margin-top:-4px;">Home Nursing Helpline</div>',
            '        </td>',
            '        <td style="width:90px;vertical-align:middle;text-align:right;">' + stethImg + '</td>',
            '      </tr></tbody>',
            '    </table>',
            '    <div style="text-align:center;margin-top:4px;">',
            '      <div style="font-size:12px;font-weight:500;">' + esc(addressLine1) + '</div>',
            '      <div style="font-size:12px;font-weight:500;">Mob : ' + esc(mobileNumbers) + ' &nbsp; Email : ' + esc(email) + '</div>',
            '    </div>',
            '  </td>',
            '</tr>'
        ].join('');
    }

    function buildTitleRow(formTitle, serialNo, date) {
        return [
            '<tr>',
            '  <td colspan="2" style="padding:8px 12px;border:1px solid #aaa;">',
            '    <table style="width:100%;border-collapse:collapse;table-layout:fixed;">',
            '      <tbody><tr>',
            '        <td style="width:60%;text-align:right;padding-right:12px;vertical-align:middle;">',
            '          <span style="color:red;font-size:15px;font-weight:700;">' + esc(formTitle) + '</span>',
            '        </td>',
            '        <td style="width:40%;vertical-align:middle;">',
            '          <table style="width:100%;border-collapse:collapse;">',
            '            <tbody>',
            '              <tr>',
            '                <td style="font-size:13px;white-space:nowrap;padding:2px 4px;">No :</td>',
            '                <td style="border-bottom:1px solid #555;font-size:13px;padding:2px 4px;">' + esc(serialNo) + '</td>',
            '              </tr>',
            '              <tr>',
            '                <td style="font-size:13px;white-space:nowrap;padding:4px 4px 2px;">Date :</td>',
            '                <td style="border-bottom:1px solid #555;font-size:13px;padding:4px 4px 2px;">' + esc(date) + '</td>',
            '              </tr>',
            '            </tbody>',
            '          </table>',
            '        </td>',
            '      </tr></tbody>',
            '    </table>',
            '  </td>',
            '</tr>'
        ].join('');
    }

    function buildFieldRows(fields) {
        return fields.map(function (f) {
            return [
                '<tr>',
                '  <td style="width:35%;padding:7px 10px;border:1px solid #aaa;font-size:13px;font-weight:600;',
                '    word-break:break-word;vertical-align:top;">' + esc(f.label) + '</td>',
                '  <td style="width:65%;padding:7px 10px;border:1px solid #aaa;font-size:13px;',
                '    word-break:break-word;vertical-align:top;">' + esc(f.value) + '</td>',
                '</tr>'
            ].join('');
        }).join('');
    }

    function buildTermsRow(termsTitle, terms, declarationText, clientSignSrc, authorizedSignSrc) {
        var termsItems = terms.map(function (t, i) {
            return [
                '<div style="display:table;width:100%;margin-top:8px;">',
                '  <div style="display:table-cell;width:36px;font-weight:700;text-align:center;vertical-align:top;">' + (i + 1) + '.</div>',
                '  <div style="display:table-cell;font-size:12px;line-height:1.55;vertical-align:top;">' + esc(t) + '</div>',
                '</div>'
            ].join('');
        }).join('');

        var clientSignHTML = clientSignSrc
            ? '<img src="' + clientSignSrc + '" style="max-width:130px;max-height:55px;object-fit:contain;display:block;" />'
            : '<div style="height:55px;"></div>';

        var authSignHTML = authorizedSignSrc
            ? '<img src="' + authorizedSignSrc + '" style="max-width:130px;max-height:55px;object-fit:contain;display:block;" />'
            : '<div style="height:55px;"></div>';

        return [
            '<tr>',
            '  <td colspan="2" style="padding:12px 12px;border:1px solid #aaa;">',
            '    <div style="font-weight:700;font-size:13px;margin-bottom:4px;">' + esc(termsTitle) + '</div>',
            '    ' + termsItems,
            '    <div style="font-weight:700;font-size:13px;margin-top:14px;margin-bottom:4px;">Declaration</div>',
            '    <div style="font-size:12px;line-height:1.55;margin-bottom:18px;">' + esc(declarationText) + '</div>',
            '    <table style="width:100%;border-collapse:collapse;table-layout:fixed;">',
            '      <tbody><tr>',
            '        <td style="width:33%;vertical-align:bottom;padding-bottom:4px;">',
            '          ' + clientSignHTML,
            '          <div style="border-top:1px solid #555;margin-top:4px;padding-top:3px;font-size:12px;">Signature of the Client</div>',
            '        </td>',
            '        <td style="width:34%;"></td>',
            '        <td style="width:33%;vertical-align:bottom;padding-bottom:4px;">',
            '          ' + authSignHTML,
            '          <div style="border-top:1px solid #555;margin-top:4px;padding-top:3px;font-size:12px;">Authorised Signatory</div>',
            '        </td>',
            '      </tr></tbody>',
            '    </table>',
            '  </td>',
            '</tr>'
        ].join('');
    }

    /* ------------------------------------------------------------------ */
    /*  Public API                                                          */
    /* ------------------------------------------------------------------ */

    /**
     * generateRegistrationPDF(config)
     *
     * config = {
     *   filename         : string,
     *   formTitle        : string,           e.g. "OLD AGE ADMISSION FORM:"
     *   serialNo         : string,
     *   date             : string,           e.g. "2025-07-01"
     *   fields           : [{label, value}], form rows to print
     *   termsTitle       : string,
     *   terms            : string[],         plain-text terms (numbered automatically)
     *   declarationText  : string,
     *   clientSignSrc    : string,           data URI or ''
     *   authorizedSignSrc: string,           data URI or ''
     *   addressLine1     : string,
     *   mobileNumbers    : string,
     *   email            : string
     * }
     */
    window.generateRegistrationPDF = async function (config) {
        var baseUrl = window.location.origin;

        /* Load logos as data URIs so html2canvas never has CORS issues */
        var logoResults = await Promise.all([
            fetchAsDataUri(baseUrl + '/images/plus%20icon.png'),
            fetchAsDataUri(baseUrl + '/images/stethoscope.png')
        ]);
        var logoPlus  = logoResults[0];
        var logoSteth = logoResults[1];

        /* Build the A4 HTML */
        var innerHTML = [
            buildHeader(logoPlus, logoSteth, config.addressLine1, config.mobileNumbers, config.email),
            buildTitleRow(config.formTitle, config.serialNo, config.date),
            buildFieldRows(config.fields),
            buildTermsRow(config.termsTitle, config.terms, config.declarationText,
                          config.clientSignSrc, config.authorizedSignSrc)
        ].join('');

        var wrapper = [
            '<div style="',
            '  width:754px;',
            '  font-family:Arial,Helvetica,sans-serif;',
            '  background:#fff;',
            '  padding:20px;',
            '  box-sizing:border-box;',
            '">',
            '  <table style="',
            '    width:100%;',
            '    border-collapse:collapse;',
            '    border:1px solid #aaa;',
            '    table-layout:fixed;',
            '  ">',
            '    <tbody>',
            '      ' + innerHTML,
            '    </tbody>',
            '  </table>',
            '</div>'
        ].join('');

        /* Mount off-screen container */
        var container = document.createElement('div');
        container.setAttribute('aria-hidden', 'true');
        container.style.cssText = [
            'position:absolute',
            'left:-9999px',
            'top:0',
            'width:794px',
            'background:#fff',
            'z-index:-1'
        ].join(';');
        container.innerHTML = wrapper;
        document.body.appendChild(container);

        /* Wait for embedded signature/logo images to decode */
        await waitForImages(container);

        var options = {
            margin:     [0.3, 0.25, 0.15, 0.25],
            filename:   config.filename || 'registration.pdf',
            image:      { type: 'jpeg', quality: 0.98 },
            html2canvas: {
                scale:       2,
                useCORS:     true,
                allowTaint:  true,
                logging:     false,
                scrollX:     0,
                scrollY:     0,
                windowWidth: 794
            },
            jsPDF:      { unit: 'in', format: 'letter', orientation: 'portrait' },
            pagebreak:  { mode: ['avoid-all', 'css', 'legacy'] }
        };

        try {
            await html2pdf().from(container).set(options).save();
        } finally {
            document.body.removeChild(container);
        }
    };

})(window);
