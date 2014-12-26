using System.Collections.Generic;
using metahub.imperative.types;

namespace metahub.imperative.schema {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Zone
{
	List<List<Expression>> divisions = new List<List<Expression>>();
	List<Expression> target;
	Dictionary<string, List<Expression>> blocks;

    public Zone(List<Expression> target, Dictionary<string, List<Expression>> blocks)
	{
		this.target = target;
		this.blocks = blocks;
	}
		
	public List<Expression> divide (string block_name = null, List<Expression> division = null) {
		division = add_zone(division);
		if (block_name != null)
			blocks[block_name] = division;		
			
		return division;
	}
	
	public List<Expression> add_zone (List<Expression> zone = null) {
		if (zone == null)
			zone = new List<Expression>();
			
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