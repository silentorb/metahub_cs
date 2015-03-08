#include "stdafx.h"
#include "test/Pizza.h"

namespace test {

	void Pizza::add(std::string topping) {
		if (topping == null)
			return;

		toppings.push_back(topping);
	}
}
