package metahub.imperative.types ;

/**
 * @author Christopher W. Johnson
 */

 @:enum
abstract Expression_Type(int) {
	// Expressions
  int literal = 1;
  int property = 2;
  int variable = 3;
  int function_call = 4;
	int instantiate = 5;
	int parent_class = 6;
	
	int create_array = 8;
	int null_value = 9;
	int self = 10;

	int path = 200;

	// Statements
	int statement = 99;
	int namespace = 100;
	int class_definition = 101;
	int function_definition = 102;
	int flow_control = 103;
	int assignment = 104;
	int declare_variable = 105;
	int scope = 106;
	int insert = 107;
}