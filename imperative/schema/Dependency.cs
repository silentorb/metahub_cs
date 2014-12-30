using metahub.logic.schema;

namespace metahub.imperative.schema
{
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Dependency
{
	public Rail rail;
	public bool allow_ambient = true;

	public Dependency(Rail rail)
	{
		this.rail = rail;
	}
	
}
}