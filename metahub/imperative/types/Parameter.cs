using metahub.logic.schema;

namespace metahub.imperative.types
{

/**
 * @author Christopher W. Johnson
 */
public class Parameter {
	public string name;
	public Signature signature;

	public Parameter(string name, Signature signature)
	{
		this.name = name;
		this.signature = signature;
	}
}
}