using metahub.imperative.types.Expression;

namespace a {
/**
 * ...
 * @author Christopher W. Johnson
 */
class Zone
{
	List<List<Expression>> divisions = new List<List<Expression>>();
	List<Expression> target;
	Map blocks<string, List<metahub.imperative.types.Expression>>;
	
	public Zone(List<Expression> target, blocks)
	{
		this.target = target;
		this.blocks = blocks;
	}
		
	public void divide (string block_name = null, List<Expression> division = null) {
		division = add_zone(division);
		if (block_name != null)
			blocks[block_name] = division;		
			
		return division;
	}
	
	public List<Expression> add_zone (List<Expression> zone = null) {
		if (zone == null)
			zone = [];
			
		divisions.Add(zone);
		return zone;
	}
	
	public void flatten () {
		foreach (var division in divisions) {
			foreach (var expression in division) {
				target.Add(expression);
			}
		}
		
		divisions = new List<List<Expression>>();
	}
	
}}