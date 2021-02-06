using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace SourcetrailDotnetIndexer
{
    class ILParser
    {
        #region the numer of argument-bytes that follows each opcode
        
        private static readonly Dictionary<ushort, int> OpcodeArgumentBytes = new Dictionary<ushort, int>
        {
            {   0x00, 0 }, // nop
            {   0x01, 0 }, // break
            {   0x02, 0 }, // ldarg.0
            {   0x03, 0 }, // ldarg.1
            {   0x04, 0 }, // ldarg.2
            {   0x05, 0 }, // ldarg.3
            {   0x06, 0 }, // ldloc.0
            {   0x07, 0 }, // ldloc.1
            {   0x08, 0 }, // ldloc.2
            {   0x09, 0 }, // ldloc.3
            {   0x0A, 0 }, // stloc.0
            {   0x0B, 0 }, // stloc.1
            {   0x0C, 0 }, // stloc.2
            {   0x0D, 0 }, // stloc.3
            {   0x0E, 1 }, // ldarg.s
            {   0x0F, 1 }, // ldarga.s
            {   0x10, 1 }, // starg.s
            {   0x11, 1 }, // ldloc.s
            {   0x12, 1 }, // ldloca.s
            {   0x13, 1 }, // stloc.s
            {   0x14, 0 }, // ldnull
            {   0x15, 0 }, // ldc.i4.m1
            {   0x16, 0 }, // ldc.i4.0
            {   0x17, 0 }, // ldc.i4.1
            {   0x18, 0 }, // ldc.i4.2
            {   0x19, 0 }, // ldc.i4.3
            {   0x1A, 0 }, // ldc.i4.4
            {   0x1B, 0 }, // ldc.i4.5
            {   0x1C, 0 }, // ldc.i4.6
            {   0x1D, 0 }, // ldc.i4.7
            {   0x1E, 0 }, // ldc.i4.8
            {   0x1F, 1 }, // ldc.i4.s
            {   0x20, 4 }, // ldc.i4
            {   0x21, 8 }, // ldc.i8
            {   0x22, 4 }, // ldc.r4
            {   0x23, 8 }, // ldc.r8
            {   0x25, 0 }, // dup
            {   0x26, 0 }, // pop
            {   0x27, 4 }, // jmp
            {   0x28, 4 }, // call
            {   0x29, 4 }, // calli
            {   0x2A, 0 }, // ret
            {   0x2B, 1 }, // br.s
            {   0x2C, 1 }, // brfalse.s
            {   0x2D, 1 }, // brtrue.s
            {   0x2E, 1 }, // beq.s
            {   0x2F, 1 }, // bge.s
            {   0x30, 1 }, // bgt.s
            {   0x31, 1 }, // ble.s
            {   0x32, 1 }, // blt.s
            {   0x33, 1 }, // bne.un.s
            {   0x34, 1 }, // bge.un.s
            {   0x35, 1 }, // bgt.un.s
            {   0x36, 1 }, // ble.un.s
            {   0x37, 1 }, // blt.un.s
            {   0x38, 4 }, // br
            {   0x39, 4 }, // brfalse
            {   0x3A, 4 }, // brtrue
            {   0x3B, 4 }, // beq
            {   0x3C, 4 }, // bge
            {   0x3D, 4 }, // bgt
            {   0x3E, 4 }, // ble
            {   0x3F, 4 }, // blt
            {   0x40, 4 }, // bne.un
            {   0x41, 4 }, // bge.un
            {   0x42, 4 }, // bgt.un
            {   0x43, 4 }, // ble.un
            {   0x44, 4 }, // blt.un
            {   0x45, 4 }, // switch
            {   0x46, 0 }, // ldind.i1
            {   0x47, 0 }, // ldind.u1
            {   0x48, 0 }, // ldind.i2
            {   0x49, 0 }, // ldind.u2
            {   0x4A, 0 }, // ldind.i4
            {   0x4B, 0 }, // ldind.u4
            {   0x4C, 0 }, // ldind.i8
            {   0x4D, 0 }, // ldind.i
            {   0x4E, 0 }, // ldind.r4
            {   0x4F, 0 }, // ldind.r8
            {   0x50, 0 }, // ldind.ref
            {   0x51, 0 }, // stind.ref
            {   0x52, 0 }, // stind.i1
            {   0x53, 0 }, // stind.i2
            {   0x54, 0 }, // stind.i4
            {   0x55, 0 }, // stind.i8
            {   0x56, 0 }, // stind.r4
            {   0x57, 0 }, // stind.r8
            {   0x58, 0 }, // add
            {   0x59, 0 }, // sub
            {   0x5A, 0 }, // mul
            {   0x5B, 0 }, // div
            {   0x5C, 0 }, // div.un
            {   0x5D, 0 }, // rem
            {   0x5E, 0 }, // rem.un
            {   0x5F, 0 }, // and
            {   0x60, 0 }, // or
            {   0x61, 0 }, // xor
            {   0x62, 0 }, // shl
            {   0x63, 0 }, // shr
            {   0x64, 0 }, // shr.un
            {   0x65, 0 }, // neg
            {   0x66, 0 }, // not
            {   0x67, 0 }, // conv.i1
            {   0x68, 0 }, // conv.i2
            {   0x69, 0 }, // conv.i4
            {   0x6A, 0 }, // conv.i8
            {   0x6B, 0 }, // conv.r4
            {   0x6C, 0 }, // conv.r8
            {   0x6D, 0 }, // conv.u4
            {   0x6E, 0 }, // conv.u8
            {   0x6F, 4 }, // callvirt
            {   0x70, 4 }, // cpobj
            {   0x71, 4 }, // ldobj
            {   0x72, 4 }, // ldstr
            {   0x73, 4 }, // newobj
            {   0x74, 4 }, // castclass
            {   0x75, 4 }, // isinst
            {   0x76, 0 }, // conv.r.un
            {   0x79, 4 }, // unbox
            {   0x7A, 0 }, // throw
            {   0x7B, 4 }, // ldfld
            {   0x7C, 4 }, // ldflda
            {   0x7D, 4 }, // stfld
            {   0x7E, 4 }, // ldsfld
            {   0x7F, 4 }, // ldsflda
            {   0x80, 4 }, // stsfld
            {   0x81, 4 }, // stobj
            {   0x82, 0 }, // conv.ovf.i1.un
            {   0x83, 0 }, // conv.ovf.i2.un
            {   0x84, 0 }, // conv.ovf.i4.un
            {   0x85, 0 }, // conv.ovf.i8.un
            {   0x86, 0 }, // conv.ovf.u1.un
            {   0x87, 0 }, // conv.ovf.u2.un
            {   0x88, 0 }, // conv.ovf.u4.un
            {   0x89, 0 }, // conv.ovf.u8.un
            {   0x8A, 0 }, // conv.ovf.i.un
            {   0x8B, 0 }, // conv.ovf.u.un
            {   0x8C, 4 }, // box
            {   0x8D, 4 }, // newarr
            {   0x8E, 0 }, // ldlen
            {   0x8F, 4 }, // ldelema
            {   0x90, 0 }, // ldelem.i1
            {   0x91, 0 }, // ldelem.u1
            {   0x92, 0 }, // ldelem.i2
            {   0x93, 0 }, // ldelem.u2
            {   0x94, 0 }, // ldelem.i4
            {   0x95, 0 }, // ldelem.u4
            {   0x96, 0 }, // ldelem.i8
            {   0x97, 0 }, // ldelem.i
            {   0x98, 0 }, // ldelem.r4
            {   0x99, 0 }, // ldelem.r8
            {   0x9A, 0 }, // ldelem.ref
            {   0x9B, 0 }, // stelem.i
            {   0x9C, 0 }, // stelem.i1
            {   0x9D, 0 }, // stelem.i2
            {   0x9E, 0 }, // stelem.i4
            {   0x9F, 0 }, // stelem.i8
            {   0xA0, 0 }, // stelem.r4
            {   0xA1, 0 }, // stelem.r8
            {   0xA2, 0 }, // stelem.ref
            {   0xA3, 4 }, // ldelem
            {   0xA4, 4 }, // stelem
            {   0xA5, 4 }, // unbox.any
            {   0xB3, 0 }, // conv.ovf.i1
            {   0xB4, 0 }, // conv.ovf.u1
            {   0xB5, 0 }, // conv.ovf.i2
            {   0xB6, 0 }, // conv.ovf.u2
            {   0xB7, 0 }, // conv.ovf.i4
            {   0xB8, 0 }, // conv.ovf.u4
            {   0xB9, 0 }, // conv.ovf.i8
            {   0xBA, 0 }, // conv.ovf.u8
            {   0xC2, 4 }, // refanyval
            {   0xC3, 0 }, // ckfinite
            {   0xC6, 4 }, // mkrefany
            {   0xD0, 4 }, // ldtoken
            {   0xD1, 0 }, // conv.u2
            {   0xD2, 0 }, // conv.u1
            {   0xD3, 0 }, // conv.i
            {   0xD4, 0 }, // conv.ovf.i
            {   0xD5, 0 }, // conv.ovf.u
            {   0xD6, 0 }, // add.ovf
            {   0xD7, 0 }, // add.ovf.un
            {   0xD8, 0 }, // mul.ovf
            {   0xD9, 0 }, // mul.ovf.un
            {   0xDA, 0 }, // sub.ovf
            {   0xDB, 0 }, // sub.ovf.un
            {   0xDC, 0 }, // endfinally
            {   0xDD, 4 }, // leave
            {   0xDE, 1 }, // leave.s
            {   0xDF, 0 }, // stind.i
            {   0xE0, 0 }, // conv.u
            { 0xFE00, 0 }, // arglist
            { 0xFE01, 0 }, // ceq
            { 0xFE02, 0 }, // cgt
            { 0xFE03, 0 }, // cgt.un
            { 0xFE04, 0 }, // clt
            { 0xFE05, 0 }, // clt.un
            { 0xFE06, 4 }, // ldftn
            { 0xFE07, 4 }, // ldvirtftn
            { 0xFE09, 2 }, // ldarg
            { 0xFE0A, 2 }, // ldarga
            { 0xFE0B, 2 }, // starg
            { 0xFE0C, 2 }, // ldloc
            { 0xFE0D, 2 }, // ldloca
            { 0xFE0E, 2 }, // stloc
            { 0xFE0F, 0 }, // localloc
            { 0xFE11, 0 }, // endfilter
            { 0xFE12, 0 }, // unaligned
            { 0xFE13, 0 }, // volatile
            { 0xFE14, 0 }, // tail
            { 0xFE15, 4 }, // Initobj
            { 0xFE16, 4 }, // constrained
            { 0xFE17, 0 }, // cpblk
            { 0xFE18, 0 }, // initblk
            { 0xFE19, 0 }, // no
            { 0xFE1A, 0 }, // rethrow
            { 0xFE1C, 4 }, // sizeof
            { 0xFE1D, 0 }, // Refanytype
            { 0xFE1E, 0 }, // readonly
        };

        #endregion

        private static readonly Dictionary<ushort, OpCode> opcodeTable = BuildOpcodeTable();

        private readonly MethodReferenceVisitor referenceVisitor;

        /// <summary>
        /// Creates a new instance of the <see cref="ILParser"/>
        /// </summary>
        /// <param name="visitor">The visitor that receives notifications of found references in the IL-code</param>
        public ILParser(MethodReferenceVisitor visitor)
        {
            referenceVisitor = visitor;
        }

        /// <summary>
        /// Parses the IL-code of the specified method and invokes the respective methods of the 
        /// <see cref="MethodReferenceVisitor"/> provided in the constructor
        /// </summary>
        /// <param name="method">The method that should be parsed</param>
        /// <param name="methodId">SymbolId of the method</param>
        /// <param name="classId">SymbolId of the class, <b>method</b> is a member of</param>
        public void Parse(MethodBase method, int methodId, int classId)
        {
            if (method is null)
                throw new ArgumentNullException(nameof(method));

            var body = method.GetMethodBody();
            if (body == null)
                return;
            var t = method.DeclaringType;
            var opList = new List<OpCode>();    // just for debugging purposes, can be removed
            var il = body.GetILAsByteArray();
            var i = 0;
            while (i < il.Length)
            {
                if (!opcodeTable.TryGetValue(il[i], out OpCode opcode))
                {
                    Console.WriteLine("Unrecognized opcode: 0x{0:x} in {1}.{2}(...)", il[i], t.FullName, method.Name);
                }
                else
                {
                    ushort opval = (ushort)opcode.Value;
                    if (opcode == OpCodes.Prefix1)
                    {
                        opval = (ushort)((opval << 8) + il[i + 1]);
                        if (!opcodeTable.TryGetValue(opval, out opcode))
                        {
                            Console.WriteLine("Unrecognized opcode: 0x{0:x} in {1}.{2}(...)", opval, t.FullName, method.Name);
                        }
                    }
                    opList.Add(opcode);
                    if (opcode.FlowControl == FlowControl.Call)
                    {
                        try
                        {
                            var mb = method.Module.ResolveMethod(
                                BitConverter.ToInt32(il, i + opcode.Size),
                                t.IsGenericType || t.IsGenericTypeDefinition ? method.DeclaringType.GetGenericArguments() : null,
                                method.IsGenericMethod || method.IsGenericMethodDefinition ? method.GetGenericArguments() : null);
                            referenceVisitor.VisitMethodCall(method, mb, methodId, classId);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Cannot resolve method-call in method '{0}.{1}': {2}",
                                method.DeclaringType.Name, method.Name, ex.Message);
                        }
                    }
                    else if (opcode.OperandType == OperandType.InlineField && opcode.OpCodeType != OpCodeType.Prefix)
                    {
                        try
                        {
                            var fi = method.Module.ResolveField(
                                BitConverter.ToInt32(il, i + opcode.Size),
                                t.IsGenericType || t.IsGenericTypeDefinition ? method.DeclaringType.GetGenericArguments() : null,
                                method.IsGenericMethod || method.IsGenericMethodDefinition ? method.GetGenericArguments() : null);
                            referenceVisitor.VisitFieldReference(fi, methodId, classId);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Cannot resolve field in method '{0}.{1}': {2}",
                                method.DeclaringType.Name, method.Name, ex.Message);
                        }
                    }
                    else if (opcode.OperandType == OperandType.InlineType && opcode.OpCodeType != OpCodeType.Prefix)
                    {
                        try
                        {
                            var referencedType = method.Module.ResolveType(
                                BitConverter.ToInt32(il, i + opcode.Size),
                                t.IsGenericType || t.IsGenericTypeDefinition ? method.DeclaringType.GetGenericArguments() : null,
                                method.IsGenericMethod || method.IsGenericMethodDefinition ? method.GetGenericArguments() : null);
                            referenceVisitor.VisitTypeReference(referencedType, methodId, classId);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Cannot resolve type in method '{0}.{1}': {2}",
                                method.DeclaringType.Name, method.Name, ex.Message);
                        }
                    }
                    else if (opcode.OperandType == OperandType.InlineMethod)
                    {
                        var mb = method.Module.ResolveMethod(
                                BitConverter.ToInt32(il, i + opcode.Size),
                                t.IsGenericType || t.IsGenericTypeDefinition ? method.DeclaringType.GetGenericArguments() : null,
                                method.IsGenericMethod || method.IsGenericMethodDefinition ? method.GetGenericArguments() : null);
                        referenceVisitor.VisitMethodReference(method, mb, methodId, classId);
                    }
                }
                i += opcode.Size;
                if (OpcodeArgumentBytes.TryGetValue((ushort)opcode.Value, out int argLength))
                {
                    if (opcode == OpCodes.Switch)   // seems to be the only opcode with variable argument size
                    {
                        var numJumps = BitConverter.ToInt32(il, i);
                        i += numJumps * 4;
                    }
                    i += argLength;
                }
                else
                    Console.WriteLine("No length for opcode {0} (0x{1:x})", opcode, opcode.Value);
            }
        }

        private static Dictionary<ushort, OpCode> BuildOpcodeTable()
        {
            var tbl = new Dictionary<ushort, OpCode>();
            foreach (var field in typeof(OpCodes).GetFields())
            {
                var code = (OpCode)field.GetValue(null);
                tbl.Add((ushort)code.Value, code);
            }
            return tbl;
        }
    }
}
