var test = {}

test.Pizza = function() {}
test.Pizza.prototype = {
	toppings: [],
	add: function(topping) {
		var x = 0
		if (topping == null)
			return
		else
			x = 1
		
		this.toppings.push(topping)
	}
}
