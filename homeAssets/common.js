
function funSearch() {
    var search_str = jQuery('#search_str').val();
    var baseUrl = document.getElementById('baseUrl').getAttribute('data-url');
    if (search_str != '' && search_str.length > 3) {
        window.location.href = baseUrl + '?searchStr=' + encodeURIComponent(search_str);
    }
}

function sort_by() {
    var sort_by_value = jQuery('#sort_by_value').val();
    jQuery('#sort').val(sort_by_value);
    jQuery('#categoryFilter').submit();
}

function setColor(color, type) {
    $('#col').css('border', '1px solid #F63440')
    if (type == 1) {
        jQuery('#color_filter').val(color);
    } else {
        jQuery('#color_filter').val(color + ':' + color_str);

    }

    jQuery('#categoryFilter').submit();
}

function sort_price_filter() {
    var input_start_value = jQuery('#filter_price_start_input').val();
    var input_end_value = jQuery('#filter_price_end_input').val();

    jQuery('#filter_price_start').val(input_start_value);
    jQuery('#filter_price_end').val(input_end_value);
    jQuery('#categoryFilter').submit();
}

function showColor(size) {
    $('#size_id').val(size);
    $('.product_color').hide();
    $('.size_' + size).show();
    $('.size_link').css('color', '#C5C5C5');
    $('#size_' + size).css('color', 'red');
}
function select_color(color) {
    $('#color_id').val(color);
}




function limitWords(elementId, limit) {
    var paragraph = document.getElementById(elementId);
    var words = paragraph.innerHTML.split(' ').slice(0, limit).join(' ');
    paragraph.innerHTML = words + '...';
}

// Call the function to limit the words in the paragraph
limitWords('limitedParagraph', 50); // Adjust the limit as needed

limitWords('userid', 10);