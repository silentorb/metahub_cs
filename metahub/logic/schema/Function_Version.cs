package metahub.logic.schema;

/**
 * ...
 * @author Christopher W. Johnson
 */
class Function_Version {
	public Signature input_signature;
	public Signature output_signature;
	
	public Function_Version(Signature input, Signature output) {
		input_signature = input;
		output_signature = output;
	}
}