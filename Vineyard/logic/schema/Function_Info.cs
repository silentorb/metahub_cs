using System;
using System.Collections.Generic;
using metahub.logic.nodes;
using metahub.schema;

namespace metahub.logic.schema {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Function_Info
{
	public string name;
	public List<Signature> versions = new List<Signature>();

	public Function_Info(string name, List<Signature> versions)
	{
		this.name = name;
		this.versions = versions;
	}
	
	public Signature add_version (Signature signature) {
		versions.Add(signature);
		return signature;
	}
	
	public Signature get_signature (Signature input) {
		foreach (var version in versions) {
			if (match_signatures(version.parameters[0], input))
				return version;
		}
		
		throw new Exception("Could not find matching signature for function: " + name);
	}
	
	public static bool match_signatures (Signature first, Signature second) {
	    if (first.type == Kind.reference || first.is_list)
	    {
	        return first.type == second.type && 
                (first.trellis == null || second.trellis == null || first.trellis == second.trellis);
	    }
	    
	    return first.type == second.type;
	}
	
}}