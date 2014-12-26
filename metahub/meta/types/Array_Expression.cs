using System.Collections.Generic;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.meta.types {
/**
 * @author Christopher W. Johnson
 */
public class Array_Expression : Node{
	public List<Node> children;

	public Array_Expression(List<Node> children = null)
		:base(Node_Type.array)
    {
		this.children = children ?? new List<Node>();
	}
	
	override public Signature get_signature ()
	{
		return new Signature { type = Kind.list, rail = null };
	}
}}