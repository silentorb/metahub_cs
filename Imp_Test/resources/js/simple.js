var test = {}

test.Pizza = function() {}
test.Pizza.prototype = {
	toppings: [],
	add: function(topping) {
		if (topping == null)
			return

		this.toppings.push(topping)
	}
}
