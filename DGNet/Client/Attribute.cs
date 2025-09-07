namespace DGNet.Client;


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ClientAttribute(Type type) : Attribute { }


[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class PropAttribute() : Attribute {}