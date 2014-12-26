package metahub.imperative.types ;
using metahub.logic.schema.Signature;

/**
 * @author Christopher W. Johnson
 */

class Parameter {
	public string name;
	public Signature signature;

	public Parameter(string name, Signature signature)
	{
		this.name = name;
		this.signature = signature;
	}
}