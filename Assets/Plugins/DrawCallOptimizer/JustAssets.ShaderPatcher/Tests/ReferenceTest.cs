using System.Collections.Generic;
using FluentAssert;
using JustAssets.ShaderPatcher.Parts;
using NUnit.Framework;

namespace JustAssets.ShaderPatcher.Tests
{
    public class ReferenceTest
    {
        [Test]
        public void TestNoRegisters_ReturnsCorrectPath()
        {
            Struct structInput = CreateStructWithoutRegisters(out var methodParams);

            var success = ShaderPatcher.TryFindTexCoord(2, methodParams, out var result, "uv2_atlas", new List<Struct> {structInput});
            success.ShouldBeTrue();
            result.ShouldBeEqualTo("i.uv2_atlas");
        }

        [Test]
        public void TestNoRegisters_ReturnsCorrectPathForPosition()
        {
            Struct structInput = CreateStructWithoutRegisters(out var methodParams);

            var success = ShaderPatcher.TryFindTexCoord(0, methodParams, out var result, "", new List<Struct> {structInput});
            success.ShouldBeTrue();
            result.ShouldBeEqualTo("i.uv_texcoord");
        }

        [Test]
        public void TestRegisters_ReturnsCorrectPath()
        {
            var s = new List<IParameter>
            {
                new Parameter(new Variable("float2"), new Variable("TEXCOORD0"), new Variable("tc_Control"), null),
                new Parameter(new Variable("float4"), new Variable("TEXCOORD1"), new Variable("tc_Splat01"), null),
                new Parameter(new Variable("float3"), new Variable("TEXCOORD2"), new Variable("atlas"), null),
                new Parameter(new Variable("float3"), new Variable("TEXCOORD3"), new Variable("tc_Splat23"), null),
                new UnparsedParameter(new Untouched("UNITY_FOG_COORDS(4)", 0))
            };

            var structInput = new Struct("\r\n", 2, "Input", s);

            var methodParams = new List<IParameter>
            {
                new Parameter(new Variable("Input"), null, new Variable("IN"), null),
                new Parameter(new Variable("SurfaceOutputStandard"), null, new Variable("o"), new List<string> {"inout"})
            };

            var success = ShaderPatcher.TryFindTexCoord(2, methodParams, out var result, "atlas", new List<Struct> {structInput});

            success.ShouldBeTrue();
            result.ShouldBeEqualTo("IN.atlas");
        }

        [Test]
        public void TestUV0RegisterOnMethod_ReturnsCorrectPath()
        {
            var methodParams = GetMethodParamsWithRegisters();

            var success = ShaderPatcher.TryFindTexCoord(0, methodParams, out var result, null, new List<Struct>());

            success.ShouldBeTrue();
            result.ShouldBeEqualTo("vertex");
        }

        [Test]
        public void TestUV2RegisterOnMethod_ReturnsCorrectPath()
        {
            var methodParams = GetMethodParamsWithRegisters();

            var success = ShaderPatcher.TryFindTexCoord(2, methodParams, out var result, "atlas", new List<Struct>());

            success.ShouldBeTrue();
            result.ShouldBeEqualTo("atlas");
        }

        private static Struct CreateStructWithoutRegisters(out List<IParameter> methodParams)
        {
            var s = new List<IParameter>
            {
                new Parameter(new Variable("float2"), null, new Variable("uv_texcoord"), null),
                new Parameter(new Variable("float4"), null, new Variable("screenPos"), null),
                new Parameter(new Variable("float3"), null, new Variable("worldRefl"), null),
                new UnparsedParameter(new Variable("INTERNAL_DATA")),
                new Parameter(new Variable("float3"), null, new Variable("worldNormal"), null),
                new Parameter(new Variable("float3"), null, new Variable("worldPos"), null),
                new Parameter(new Variable("float4"), null, new Variable("uv2_atlas"), null)
            };

            var structInput = new Struct("\r\n", 2, "Input", s);

            methodParams = new List<IParameter>
            {
                new Parameter(new Variable("Input"), null, new Variable("i"), null),
                new Parameter(new Variable("SurfaceOutputStandardSpecular"), null, new Variable("o"), new List<string> {"inout"})
            };
            return structInput;
        }

        private static List<IParameter> GetMethodParamsWithRegisters()
        {
            var methodParams = new List<IParameter>
            {
                new Parameter(new Variable("float4"), new Variable("POSITION"), new Variable("vertex"), null),
                new Parameter(new Variable("float2"), new Variable("TEXCOOORD0"), new Variable("texcoord"), null),
                new Parameter(new Variable("float4"), new Variable("TEXCOOORD2"), new Variable("atlas"), null)
            };
            return methodParams;
        }
    }
}