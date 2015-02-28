using System.Collections.Generic;
using metahub.schema;

namespace metahub.logic.schema
{

/**
 * ...
 * @author Christopher W. Johnson
 */
public class Range_Float : IRange
{
	public float min;
	public float max;
    public int type { get; set; }
	public List<Property> path { get; set; }
	
	public Range_Float(float min, float max, List<Property> path)
	{
		this.min = min;
		this.max = max;
		this.path = path;
	}
	
}
}