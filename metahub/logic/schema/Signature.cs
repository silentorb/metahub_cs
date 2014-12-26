using metahub.schema.Kind;
using metahub.logic.schema.Rail;

namespace metahub.logic.schema {
/**
 * @author Christopher W. Johnson
 */

struct Signature {
	Kind type,
	Rail rail,
	bool is_value,
	int is_numeric
}}