package metahub.imperative.types ;
using metahub.logic.schema.Signature;

/**
 * @author Christopher W. Johnson
 */

struct Scope {
	//Expression_Type type,
	Dictionary variables<string, Signature>,
	//List<Scope> children,
	//List<Object> statements
}