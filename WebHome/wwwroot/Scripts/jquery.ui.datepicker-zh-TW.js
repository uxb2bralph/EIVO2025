/* Chinese initialisation for the jQuery UI date picker plugin. */
/* Written by Ressol (ressol@gmail.com). */
jQuery(function ($) {
	var dateSelected = false;
	$.datepicker.regional['zh-TW'] = {
		closeText: '清除',
		prevText: '&#x3c;上月',
		nextText: '下月&#x3e;',
		currentText: '今天',
		monthNames: ['一月','二月','三月','四月','五月','六月',
		'七月','八月','九月','十月','十一月','十二月'],
		monthNamesShort: ['一','二','三','四','五','六',
		'七','八','九','十','十一','十二'],
		dayNames: ['星期日','星期一','星期二','星期三','星期四','星期五','星期六'],
		dayNamesShort: ['周日','周一','周二','周三','周四','周五','周六'],
		dayNamesMin: ['日','一','二','三','四','五','六'],
		weekHeader: '周',
		dateFormat: 'yy/mm/dd',
		firstDay: 1,
		isRTL: false,
		showMonthAfterYear: true,
		beforeShow: function (input, inst) {
			dateSelected = false;
		},
		onUpdateDatepicker: function (inst) {
			debugger;
			var $target = $(event.target)
			if ($target.is('[data-handler="today"]')) {
				var d = new Date();
				inst.input.val(d.toISOString().split('T')[0].replaceAll('-', '/'));
				dateSelected = true;
				inst.input.datepicker('hide');
			}
			//var $input = $(this);
			//var btnToday = inst.dpDiv.find('button').eq(0);
			//btnToday.on('click', function () {
			//	$input.datepicker('setDate', new Date());
			//	$input.datepicker('hide');
			//});
		},
		onClose: function (dateText, inst) {
			if (!dateSelected) {
				inst.input.val('');
			}
		},
		onSelect: function (dateText, inst) {
			dateSelected = true;
		},
		yearSuffix: '年'};
	$.datepicker.setDefaults($.datepicker.regional['zh-TW']);
});
