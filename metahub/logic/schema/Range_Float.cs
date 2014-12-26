package metahub.logic.schema;

/**
 * ...
 * @author Christopher W. Johnson
 */
class Range_Float implements IRange
{
	public float min;
	public float max;
  public int type;
	public List<Tie> path;
	
	public Range_Float(float min, float max, List<Tie> path)
	{
		this.min = min;
		this.max = max;
		this.path = path;
	}
	
}