function FixedAll(obj, marginTop) {
    debugger;
    var $table = $(obj);
    var $sp = $table.scrollParent();
    var tableOffset = $table.offset().top;
    var $tableFixed = $("<table />")
                .attr('class', $table.attr('class'))
                .css({
                    position: "fixed",
                    "table-layout": "fixed",
                    display: "none",
                    "margin-top": "0px",
                    "z-index":"999999",
                });
    $table.before($tableFixed);
    $tableFixed.append($table.find(".fixheader").clone());
    $(window).bind("scroll", function() { 
    var offset = $(this).scrollTop();

                if (offset > tableOffset && $tableFixed.is(":hidden")) {
                    $tableFixed.show();
                    var p = $table.offset();
                    var offset = $sp.offset();

                    //Set the left and width to match the source table and the top to match the scroll parent
                    $tableFixed.css({
                        //left: p.left - $sp.scrollLeft() + "px",
                        top: "52px"
                            //marginTop +  (offset ? offset.top : 0) + "px",
                    }).width($table.width());

                    //Set the width of each column to match the source table
                    $.each($table.find('th, td'), function (i, th) {
                        $($tableFixed.find('th, td')[i]).width($(th).width());
                    });

                } else if (offset <= tableOffset && !$tableFixed.is(":hidden")) {
                    $tableFixed.hide();
                } else if (!$tableFixed.is(":hidden")) {
                    var p = $table.offset();
                    $tableFixed.css({
                        //left: (p.left - $sp.scrollLeft()) + "px"
                    });
                }
    });
}