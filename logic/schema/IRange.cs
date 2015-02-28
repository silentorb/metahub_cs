using System.Collections.Generic;
using metahub.schema;

namespace metahub.logic.schema
{

/**
 * @author Christopher W. Johnson
 */

public interface IRange 
{
    int type { get; set; }
	List<Property> path { get; set; }
}
}