namespace metahub.imperative.schema
{
using metahub.logic.schema.Rail;

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