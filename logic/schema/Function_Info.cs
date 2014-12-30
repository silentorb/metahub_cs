using System;
using System.Collections.Generic;
using metahub.logic.types;
using metahub.schema;

namespace metahub.logic.schema {
/**
 * ...
 * @author Christopher W. Johnson
 */
public class Function_Info
{
	public string name;
	public List<Function_Version> versions = new List<Function_Version>();

	public Function_Info(string name, List<Function_Version> versions)
	{
		this.name = name;
		this.versions = versions;
	}
	
	public Function_Version add_version (Signature input_signature, Signature output_signature) {
		Function_Version version = new Function_Version(input_signature, output_signature);
		versions.Add(version);
		return version;
	}
	
	public Signature get_signature (Function_Call call) {
		var input = call.input.get_signature();
		foreach (var version in versions) {
			if (match_signatures(version.input_signature, input))
				return version.output_signature;
		}
		
		throw new Exception("Could not find matching signature for function: " + name);
	}
	
	public static bool match_signatures (Signature first, Signature second) {
		if (first.type == Kind.reference || first.type == Kind.list)
			return first.type == second.type && (first.rail == null || first.rail == second.rail);
		else
			return first.type == second.type;
	}
	
}}