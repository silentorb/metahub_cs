using metahub.logic.schema.Range_Float;
using metahub.logic.schema.Tie;
using metahub.schema.Kind;
using metahub.imperative.types.*;

namespace a {
/**
 * ...
 * @author Christopher W. Johnson
 */
class Lair
{
	public Tie tie;
	public Dungeon dungeon;
	public string name;
	
	public Lair(Tie tie, Dungeon dungeon)
	{
		this.tie = tie;
		this.dungeon = dungeon;
		this.name = tie.name;
	}
	
	public void customize_initialize (List<Expression> block) {
		foreach (var r in tie.ranges) {
			Range_Float range = cast r;
			var reference = create_reference(range.path.Count() > 0
				? new Path(cast range.path.map((t)=> new Property_Expression(t)))
				: null
			);
			block.Add(new Assignment(reference, "=", new Function_Call("rand",
				[new Literal(range.min, {type: Kind.Float }), new Literal(range.max, {type: Kind.Float})], true)));
		}
	}
	
	public Property_Expression create_reference (Expression child = null) {
		return new Property_Expression(tie, child);
	}
}}