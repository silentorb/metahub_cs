using System.Collections.Generic;
using metahub.imperative.types;

namespace metahub.imperative.schema
{
    /**
     * ...
     * @author Christopher W. Johnson
     */
    /*
public class Zone
{
	List<List<Expression>> divisions = new List<List<Expression>>();
	Block target;
    private Dungeon dungeon;

    public Zone(Block target, Dungeon dungeon)
	{
		this.target = target;
        this.dungeon = dungeon;
	}
		
	public List<Expression> divide (string block_name = null, List<Expression> division = null) {
		division = add_division(division);
        if (!dungeon.has_block(block_name))
			dungeon.create_block(block_name, target.scope, division);		
			
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
				target.add(expression);
			}
		}
		
		divisions = new List<List<Expression>>();
	}
	
}*/
}