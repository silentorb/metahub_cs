using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using metahub.Properties;
using metahub.imperative;
using metahub.imperative.code;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.jackolantern.code;
using metahub.jackolantern.pumpkins;
using metahub.logic;
using metahub.logic.nodes;
using metahub.logic.schema;
using metahub.render;
using metahub.schema;

namespace metahub.jackolantern
{
    public class JackOLantern
    {
        public Logician logician;
        public Overlord overlord;

        public Dictionary<string, Carver> carvers = new Dictionary<string, Carver>();

        public Dictionary<string, Template> templates = null;

        public JackOLantern(Logician logician, Overlord overlord)
        {
            this.logician = logician;
            this.overlord = overlord;
            initialize_functions();
            templates = overlord.summon_snippets(Resources.jackolantern_snippets);
        }

        public void initialize_functions()
        {
            carvers["="] = new Equals(this);
            carvers["contains"] = new Contains(this);
        }

        public void run(Target target)
        {
            generate_code(target);

            foreach (var constraint in logician.constraints)
            {
               implement_constraint(constraint);
            }

            foreach (var pumpkin in logician.functions)
            {
                carve_pumpkin(pumpkin);
            }
        }

        public void carve_pumpkin(Function_Call2 pumpkin)
        {
            if (pumpkin.name[0] == '@')
                pumpkin.name = pumpkin.name.Substring(1);

            if (carvers.ContainsKey(pumpkin.name))
            {
                var carver = carvers[pumpkin.name];
                carver.carve(pumpkin);
            }
        }

        public static string get_setter_name(Portal portal)
        {
            return portal.is_list
                ? "add_" + portal.name
                : "set_" + portal.name;
        }

        public Imp get_initialize(Dungeon dungeon)
        {
            return dungeon.summon_imp("initialize");
        }

        public void implement_constraint(Constraint constraint)
        {
            //var ties = Parse.get_endpoints(constraint.first);
            foreach (var tie in constraint.endpoints.Where(tie => tie != null))
            {
                if (tie.type == Kind.list)
                {
                    List_Code.generate_constraint(constraint, this);
                }
                else
                {
                    Reference.generate_constraint(constraint, tie, this);
                }
            }
        }

        public void generate_code(Target target)
        {
            foreach (var region in overlord.railway.regions.Values)
            {
                //if (region.is_external)
                //    continue;

                var realm = new Realm(region, overlord);
                overlord.realms[realm.name] = realm;

                foreach (var rail in region.rails.Values)
                {

                    Dungeon dungeon = new Dungeon(rail, overlord, realm);
                    realm.dungeons[dungeon.name] = dungeon;
                    overlord.rail_map[rail] = dungeon;

                    if (rail.trellis.is_abstract && rail.trellis.is_value)
                        continue;

                    overlord.dungeons.Add(dungeon);
                }
            }

            foreach (var dungeon in overlord.dungeons)
            {
                dungeon.initialize();
            }

            overlord.finalize();

            var not_external = overlord.dungeons.Where(d => !d.is_external).ToArray();

            foreach (var dungeon in not_external)
            {
                dungeon.generate_code();
                Dungeon_Carver.generate_code1(this, dungeon, dungeon.rail);
            }

            foreach (var dungeon in not_external)
            {
                target.generate_rail_code(dungeon);
                Dungeon_Carver.generate_code2(this, dungeon, dungeon.rail);
            }

            overlord.summon(Resources.metahub_imp);

            if (logician.railway.regions.ContainsKey("piecemaker"))
            {
                var piece_region = logician.railway.regions["piecemaker"];
                Piece_Maker.add_functions(overlord, piece_region);
            }

            foreach (var dungeon in not_external)
            {
                target.generate_code2(dungeon);
            }
        }

        public Imp get_setter(Portal portal)
        {
            var setter = portal.dungeon.summon_imp(get_setter_name(portal)) 
                ?? Dungeon_Carver.generate_setter(portal);

            return setter;
        }

        public Expression translate(Node expression, Scope scope = null)
        {
            var result = _translate(expression, scope);
            if (expression.inputs.Count > 0)
            {
                if (result.child != null)
                    throw new Exception("Looks like a bug.");

                var input = expression.inputs[0];
                if (input.type == Node_Type.function_call)
                {
                    var function_call = (metahub.logic.nodes.Function_Call)input;
                    return new Platform_Function(function_call.name, result, null)
                    {
                        profession = new Profession(function_call.signature, overlord)
                    };
                }

                result.child = translate(expression.inputs[0], scope);
            }

            return result;
        }

        Expression _translate(Node expression, Scope scope = null)
        {
            switch (expression.type)
            {
                case Node_Type.literal:
                    return new Literal(((metahub.logic.nodes.Literal_Value)expression).value, new Profession(Kind.unknown));

                case Node_Type.function_call:
                    var function_call = expression as metahub.logic.nodes.Function_Call;
                    if (function_call != null)
                    {
                        return new Platform_Function(function_call.name, null, null)
                            {
                                profession = new Profession(function_call.signature, overlord)
                            };
                    }
                    var function_call2 = (Function_Call2) expression;
                    if (!function_call2.is_operation)
                        throw new Exception("Not supported.");

                    return new Operation(function_call2.name, translate_many(function_call2.inputs, scope));

                case Node_Type.path:
                    return convert_path(((metahub.logic.nodes.Reference_Path)expression).children, scope);

                case Node_Type.array:
                    return new Create_Array(translate_many(((metahub.logic.nodes.Array_Expression)expression).children, scope));

                case Node_Type.block:
                    return new Create_Array(translate_many(((metahub.logic.nodes.Block)expression).children, scope));

                case Node_Type.variable:
                    var variable = (metahub.logic.nodes.Variable)expression;
                    return scope.resolve(variable.name);

                case Node_Type.lambda:
                    return null;
     
                case Node_Type.property:
                    return convert_path(new List<Node> { expression });

                //case Node_Type.operation:
                //    var operation = (metahub.logic.nodes.Operation_Node)expression;
                //    return new Operation(operation.op, translate_many(operation.inputs, scope));

                default:
                    throw new Exception("Cannot convert node " + expression.type + ".");
            }
        }

        public IEnumerable<Expression> translate_many(IEnumerable<Node> nodes, Scope scope)
        {
            return nodes.Select(n => translate(n, scope));
        }

        public Expression convert_path(Node[] path, Scope scope)
        {
            if (path.Length == 0)
                throw new Exception("Cannot convert empty path.");

            if (path.Length == 1)
                return translate(path[0], scope);

            return package_path(translate_many(path, scope));
        }

        Expression package_path(IEnumerable<Expression> path)
        {
            if (path.Count() == 1)
                return path.First();

            return new Path(path);
        }

        public Expression convert_path(IList<metahub.logic.nodes.Node> path, Scope scope = null)
        {
            List<Expression> result = new List<Expression>();
            Rail rail = null;
            Dungeon dungeon = null;

            if (path.First().type == Node_Type.property)
            {
                rail = ((metahub.logic.nodes.Property_Reference)path.First()).tie.get_abstract_rail();
            }
            //else
            //{
            //    rail = ((metahub.logic.types.Array_Expression)path[0])
            //}
            foreach (var token in path)
            {
                switch (token.type)
                {
                    case Node_Type.property:
                        var property_token = (metahub.logic.nodes.Property_Reference)token;
                        if (dungeon != null)
                        {
                            var portal = dungeon.all_portals[property_token.tie.name];
                            if (portal == null)
                                throw new Exception("Portal is null: " + property_token.tie.fullname);

                            result.Add(new Portal_Expression(portal));
                            dungeon = portal.other_dungeon;
                        }
                        else
                        {
                            var tie = rail.all_ties[property_token.tie.name];
                            if (tie == null)
                                throw new Exception("tie is null: " + property_token.tie.fullname);

                            result.Add(new Portal_Expression(overlord.get_portal(tie)));
                            rail = tie.other_rail;
                        }
                        break;

                    case Node_Type.array:
                        //result.Add(translate(token, scope));
                        break;

                    case Node_Type.variable:
                        var variable = (metahub.logic.nodes.Variable)token;
                        var variable_token = scope.resolve(variable.name);
                        result.Add(variable_token);
                        var profession = variable_token.get_profession();
                        if (profession == null)
                        {
                            rail = variable_token.get_signature().rail;
                        }
                        else if (profession.dungeon != null)
                        {
                            if (profession.dungeon.rail != null)
                                rail = profession.dungeon.rail;

                            dungeon = profession.dungeon;
                        }
                        else if (profession.type == Kind.list || profession.type == Kind.reference)
                        {
                            throw new Exception("Invalid profession.");
                        }
                        break;

                    case Node_Type.function_call:
                    case Node_Type.function_scope:
                        var function_token = (metahub.logic.nodes.Function_Call)token;
                        result.Add(new Platform_Function(function_token.name, null,
                            new List<Expression>()));
                        break;

                    default:
                        throw new Exception("Invalid path token: " + token.type);
                }
            }
            return package_path(result);
        }


        public Property_Function_Call call_setter(Portal portal,Expression reference, Expression value, Expression origin)
        {
            var imp = get_setter(portal);
            return new Property_Function_Call(Property_Function_Type.set, portal, 
                origin != null && imp.parameters.Count > 1
                ? new List<Expression> { value, origin }
                : new List<Expression> { value }
             ) { reference = reference };
        }

        /*
                public Expression translate(Node expression, Scope scope = null)
        {
            var swamp = new Swamp(this);
            return swamp.translate(expression, scope);
        }
        public Expression convert_path(IList<metahub.logic.nodes.Node> path, Scope scope = null)
        {
            var swamp = new Swamp(this);
            return swamp.convert_path(path, scope);
        }
        */

    }
}
