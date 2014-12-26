package metahub.imperative.types ;
using metahub.imperative.schema.Dungeon;
using metahub.logic.schema.Rail;
using metahub.schema.Kind;
using metahub.logic.schema.Signature;

/**
 * @author Christopher W. Johnson
 */

class Function_Definition extends Anonymous_Function {
	public string name;
	public Dungeon dungeon;
	public Rail rail;
	
	public Function_Definition(string name, Dungeon dungeon, List<Parameter> parameters, List<Expression> expressions, Signature return_type = null) {
		super(parameters, expressions, return_type);		
		this.name = name;
		this.dungeon = dungeon;
		this.rail = dungeon.rail;
		if (rail != null)
			dungeon.functions.Add(this);
	}
}
