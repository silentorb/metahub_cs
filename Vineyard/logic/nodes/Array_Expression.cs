using System.Collections.Generic;
using metahub.logic.schema;
using metahub.schema;

namespace metahub.logic.nodes {
/**
 * @author Christopher W. Johnson
 */
public class Array_Expression : Node{
	public Node[] children;

	public Array_Expression(Node[] children = null)
		:base(Node_Type.array)
    {
		this.children = children ?? new Node[]{};
	}
	
	override public Signature get_signature ()
	{
		return new Signature { type = Kind.reference, trellis = null, is_list = true };
	}
}}