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
		division = add_division(division);
		if (block_name != null)
			blocks[block_name] = division;		
			
		return division;
	}
	
	List<Expression> add_division (List<Expression> division = null) {
		if (division == null)
			division = new List<Expression>();
		
		divisions.Add(division);
		return division;
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