using System;
using System.Collections.Generic;
using System.Linq;
using metahub.Properties;
using metahub.imperative.code;
using metahub.imperative.schema;
using metahub.imperative.summoner;
using metahub.imperative.types;
using metahub.logic.schema;
using metahub.render;
using metahub.schema;
using Constraint = metahub.logic.schema.Constraint;
using Literal = metahub.imperative.types.Literal;
using Logician = metahub.logic.Logician;
using Node_Type = metahub.logic.nodes.Node_Type;
using Node = metahub.logic.nodes.Node;

namespace metahub.imperative
{
    public class Overlord
    {
        public Railway railway;
        public List<Dungeon> dungeons = new List<Dungeon>();
        Dictionary<Rail, Dungeon> rail_map = new Dictionary<Rail, Dungeon>();
        public Dictionary<string, Realm> realms = new Dictionary<string, Realm>();
        public Target target;
        public Logician logician;

        public Overlord(Hub hub, string target_name)
        {
            railway = new Railway(hub, target_name);
            if (Piece_Maker.templates == null)
                Piece_Maker.initialize(this);
        }

        public void run(Logician logician, Target target)
        {
            this.logician = logician;
            this.target = target;

            logician.analyze();

            generate_code(target);

            foreach (var constraint in logician.constraints)
            {
                implement_constraint(constraint);
            }

            flatten();

            post_analyze();
        }

        public void generate_code(Target target)
        {
            foreach (var region in railway.regions.Values)
            {
                //if (region.is_external)
                //    continue;

                var realm = new Realm(region, this);
                realms[realm.name] = realm;

                foreach (var rail in region.rails.Values)
                {

                    Dungeon dungeon = new Dungeon(rail, this, realm);
                    realm.dungeons[dungeon.name] = dungeon;
                    rail_map[rail] = dungeon;

                    if (rail.trellis.is_abstract && rail.trellis.is_value)
                        continue;

                    dungeons.Add(dungeon);
                }
            }

            foreach (var dungeon in dungeons)
            {
                dungeon.initialize();
            }

            finalize();

            foreach (var dungeon in dungeons.Where(d => !d.is_external))
            {
                dungeon.generate_code1();
                target.generate_rail_code(dungeon);
                dungeon.generate_code2();
            }

            summon(Resources.metahub_imp);

            if (railway.regions.ContainsKey("piecemaker"))
            {
                var piece_region = railway.regions["piecemaker"];
                Piece_Maker.add_functions(this, piece_region);
            }

            foreach (var dungeon in dungeons.Where(d => !d.is_external))
            {
                target.generate_code2(dungeon);
            }
        }

        void finalize()
        {
            foreach (var dungeon in dungeons.Where(d => !d.is_external))
            {
                dungeon.rail.finalize();
            }
        }

        void post_analyze()
        {
            foreach (var dungeon in dungeons.Where(d => !d.is_external))
            {
                dungeon.analyze();
            }
        }

        public void flatten()
        {
            var temp = dungeons.Where(d => !d.is_external).Select(d=>d.name);

            foreach (var dungeon in dungeons.Where(d => !d.is_external))
            {
                dungeon.flatten();
            }
        }

        public Dungeon get_dungeon(Rail rail)
        {
            if (!rail_map.ContainsKey(rail))
                return null;

            return rail_map[rail];
        }

        public Dungeon get_dungeon(string name)
        {
            foreach (var realm in realms.Values)
            {
                if (realm.dungeons.ContainsKey(name))
                    return realm.dungeons[name];
            }
            
            return null;
        }

        public Dungeon get_dungeon_or_error(Rail rail)
        {
            if (!rail_map.ContainsKey(rail))
                throw new Exception("Could not find dungeon for rail " + rail.name + ".");

            return rail_map[rail];
        }

        //Node create_lambda_constraint (metahub Node.meta.types.Constraint, Scope scope) {
        //throw "";
        //var rail = get_rail(scope.trellis);
        //metahub.logic.schema.Constraint constraint = new metahub.logic.schema.Constraint(Node, this);
        //var tie = Parse.get_end_tie(constraint.reference);
        //trace("tie", tie.rail.name + "." + tie.name);
        //tie.constraints.Add(constraint);
        //constraints.Add(constraint);
        //return null;
        //}

        public Rail get_rail(Trellis trellis)
        {
            return railway.get_rail(trellis);
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

        public Expression translate(Node expression, Scope scope = null)
        {
            switch (expression.type)
            {
                case Node_Type.literal:
                    return new Literal(((metahub.logic.nodes.Literal_Value)expression).value, new Profession(Kind.unknown));

                case Node_Type.function_call:
                    var function_call = (metahub.logic.nodes.Function_Call) expression;
                    return new Function_Call(function_call.name, null, null, true)
                        {
                            profession = new Profession(function_call.signature, this)
                        };
                    //metahub.logic.types.Function_Call func = expression;
                    //return new Function_Call(func.name, [translate(func.input)]);
                    //throw new Exception("Not implemented.");

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
                //metahub.logic.types.Lambda lambda = Node;
                //return new Anonymous_(lambda.parameters.map(function(p)=> new Parameter(p.name, p.signature)),
                //lambda.expressions.map((e)=> translate(e, lambda.scope))
                //);
                //
                //case metahub.logic.types.Expression_Type.constraint:
                //return create_lambda_constraint(Node, scope);

                case Node_Type.property:
                    return convert_path(new List<Node> { expression });

                case Node_Type.operation:
                    var operation = (metahub.logic.nodes.Operation_Node)expression;
                    return new Operation(operation.op, translate_many(operation.children, scope));

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
                                throw new Exception("Portal is null: " + property_token.tie.fullname());

                            result.Add(new Portal_Expression(portal));
                            dungeon = portal.other_dungeon;
                        }
                        else
                        {
                            var tie = rail.all_ties[property_token.tie.name];
                            if (tie == null)
                                throw new Exception("tie is null: " + property_token.tie.fullname());

                            result.Add(new Tie_Expression(tie));
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
                        result.Add(new Function_Call(function_token.name, null,
                            new List<Expression>(), true));
                        break;

                    default:
                        throw new Exception("Invalid path token: " + token.type);
                }
            }
            return package_path(result);
        }

        public static Node[] simplify_path(Node[] path)
        {
            return path.Where(t => t.type == Node_Type.property).ToArray();
        }

        public Pre_Summoner pre_summon(string code, Pre_Summoner.Mode mode = Pre_Summoner.Mode.full)
        {
            var pre_summoner = new Pre_Summoner();
            pre_summoner.summon(code, mode);
            return pre_summoner;
        }

        public void summon(Pre_Summoner pre_summoner)
        {
            var summoner = new Summoner(this);
            summoner.summon(pre_summoner.output);
        }

        public void summon(string code)
        {
            var pre_summoner = pre_summon(code);
            summon(pre_summoner);
        }

        public Dungeon summon_dungeon(Template template, Summoner.Context context)
        {
            var summoner = new Summoner(this);
            return summoner.process_dungeon(template.source, context);
        }
    }
}