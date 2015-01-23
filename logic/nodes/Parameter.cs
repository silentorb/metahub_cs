using metahub.logic.schema;

namespace metahub.logic.nodes
{

public class Parameter {
	public string name;
	public Signature signature;

	public Parameter(string name, Signature signature = null)
	{
		this.name = name;
		this.signature = signature;
	}
}
}