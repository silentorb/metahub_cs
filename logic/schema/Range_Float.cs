using System.Collections.Generic;

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
	public List<Tie> path { get; set; }
	
	public Range_Float(float min, float max, List<Tie> path)
	{
		this.min = min;
		this.max = max;
		this.path = path;
	}
	
}
}