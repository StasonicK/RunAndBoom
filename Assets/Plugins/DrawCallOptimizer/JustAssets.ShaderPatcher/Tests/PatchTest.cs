using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssert;
using JustAssets.ShaderPatcher.Parts;
using NUnit.Framework;

namespace JustAssets.ShaderPatcher.Tests
{
    public class PatchTest
    {
        [Test]
        public void TestRegisters_ReturnsCorrectPath()
        {
            var unparsed = CGCommandParser.Parse(new StringSpan("UNITY_FOG_COORDS(3)"), 0, "\r\n", new List<string> {"UNITY_FOG_COORDS"}).FirstOrDefault();
            var s = new List<IParameter>
            {
                new Parameter(new Variable("float2"), new Variable("TEXCOORD0"), new Variable("tc_Control"), null),
                new Parameter(new Variable("float4"), new Variable("TEXCOORD1"), new Variable("tc_Splat01"), null),
                new Parameter(new Variable("float3"), new Variable("TEXCOORD2"), new Variable("tc_Splat23"), null),
                new UnparsedParameter(unparsed)
            };

            var structInput = new Struct("\r\n", 2, "Input", s);

            var methodParams = new List<IParameter>
            {
                new Parameter(new Variable("Input"), null, new Variable("IN"), null),
                new Parameter(new Variable("SurfaceOutputStandard"), null, new Variable("o"), new List<string> {"inout"})
            };

            var path = new ShaderPatcher(null, (prio, message) => Console.WriteLine($"[{prio}] {message}")).AddAtlasRegisterIfMissing("Input", methodParams,
                new List<Struct> {structInput});

            path.ShouldBeEqualTo("IN.atlas");
            s.Count.ShouldBeEqualTo(5);
            s[2].ShouldBeOfType<Parameter>().Cast<Parameter>().Name.Name.ShouldBeEqualTo("atlas");
        }
    }
}