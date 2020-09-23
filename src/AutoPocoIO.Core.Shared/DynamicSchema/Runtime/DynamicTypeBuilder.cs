using AutoPocoIO.DynamicSchema.Models;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace AutoPocoIO.DynamicSchema.Runtime
{
    internal class DynamicTypeBuilder
    {
        public static TypeBuilder GetTypeBuilder(Table table, Type BaseType, string assemblyName)
        {
            AssemblyName an = new AssemblyName(assemblyName);

            var interfaces = Array.Empty<Type>();

            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.RunAndCollect);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule." + table.VariableName.ToUpperInvariant());
            TypeBuilder tb;

            tb = moduleBuilder.DefineType("DynamicType." + table.VariableName.ToUpperInvariant()
                                    , TypeAttributes.Public |
                                    TypeAttributes.Class |
                                    TypeAttributes.AutoClass |
                                    TypeAttributes.AnsiClass |
                                    TypeAttributes.BeforeFieldInit |
                                    TypeAttributes.AutoLayout | TypeAttributes.Sealed
                                    , BaseType, interfaces);


            return tb;
        }

        public static PropertyBuilder CreateProperty(TypeBuilder builder, string propertyName, Type propertyType, bool notifyChanged)
        {
            FieldBuilder fieldBuilder = builder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            PropertyBuilder propertyBuilder = builder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);

            MethodBuilder getPropertyBuiler = CreatePropertyGetter(builder, fieldBuilder);
            MethodBuilder setPropertyBuiler;
            if (notifyChanged)
            {
                setPropertyBuiler = CreatePropertySetterWithNotifyChanged(builder, fieldBuilder, propertyName);
            }
            else
            {
                setPropertyBuiler = CreatePropertySetter(builder, fieldBuilder);
            }

            propertyBuilder.SetGetMethod(getPropertyBuiler);
            propertyBuilder.SetSetMethod(setPropertyBuiler);

            return propertyBuilder;
        }



        public static PropertyBuilder CreateVirtualProperty(TypeBuilder classBuilder, string propertyName, Type propertyTypeBuilder)
        {
            FieldBuilder fieldBuilder = classBuilder.DefineField("_" + propertyName, propertyTypeBuilder, FieldAttributes.Private);
            PropertyBuilder propertyBuilder = classBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyTypeBuilder, null);

            var getSetAttr = MethodAttributes.Public | MethodAttributes.Virtual;

            var mbIdGetAccessor = classBuilder.DefineMethod("get_" + propertyName, getSetAttr, propertyTypeBuilder, Type.EmptyTypes);

            var numberGetIL = mbIdGetAccessor.GetILGenerator();
            numberGetIL.Emit(OpCodes.Ldarg_0);
            numberGetIL.Emit(OpCodes.Ldfld, fieldBuilder);
            numberGetIL.Emit(OpCodes.Ret);

            var mbIdSetAccessor = classBuilder.DefineMethod("set_" + propertyName, getSetAttr, null, new Type[] { propertyTypeBuilder });

            var numberSetIL = mbIdSetAccessor.GetILGenerator();
            numberSetIL.Emit(OpCodes.Ldarg_0);
            numberSetIL.Emit(OpCodes.Ldarg_1);
            numberSetIL.Emit(OpCodes.Stfld, fieldBuilder);
            numberSetIL.Emit(OpCodes.Ret);

            propertyBuilder.SetGetMethod(mbIdGetAccessor);
            propertyBuilder.SetSetMethod(mbIdSetAccessor);

            return propertyBuilder;
        }

        private static MethodBuilder CreatePropertyGetter(TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            MethodBuilder getMethodBuilder = typeBuilder.DefineMethod("get_" + fieldBuilder.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, fieldBuilder.FieldType, Type.EmptyTypes);

            ILGenerator getIL = getMethodBuilder.GetILGenerator();

            getIL.Emit(OpCodes.Ldarg_0);
            getIL.Emit(OpCodes.Ldfld, fieldBuilder);
            getIL.Emit(OpCodes.Ret);

            return getMethodBuilder;
        }

        private static MethodBuilder CreatePropertySetterWithNotifyChanged(TypeBuilder typeBuilder, FieldBuilder fieldBuilder, string PropertyName)
        {
            //Raise
            MethodInfo m = typeof(PocoBase).GetMethod("OnRaisePropertyChanged",
                                         BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                                         null, new[] { typeof(object), typeof(string) }, null);

            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_" + fieldBuilder.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[] { fieldBuilder.FieldType });

            ILGenerator setIL = setMethodBuilder.GetILGenerator();
            setIL.Emit(OpCodes.Nop);
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldstr, PropertyName);
            setIL.Emit(OpCodes.Call, m);
            setIL.Emit(OpCodes.Nop);
            setIL.Emit(OpCodes.Ret);
            return setMethodBuilder;

        }

        private static MethodBuilder CreatePropertySetter(TypeBuilder typeBuilder, FieldBuilder fieldBuilder)
        {
            MethodBuilder setMethodBuilder = typeBuilder.DefineMethod("set_" + fieldBuilder.Name, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new Type[] { fieldBuilder.FieldType });

            ILGenerator setIL = setMethodBuilder.GetILGenerator();

            setIL.Emit(OpCodes.Ldarg_0);
            setIL.Emit(OpCodes.Ldarg_1);
            setIL.Emit(OpCodes.Stfld, fieldBuilder);
            setIL.Emit(OpCodes.Ret);

            return setMethodBuilder;
        }
    }
}
