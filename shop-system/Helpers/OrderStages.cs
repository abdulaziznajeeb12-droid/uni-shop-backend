// Har order type ke stages (tarteeb se). Naya stage chahiye to yahan badlein.
public static class OrderStages
{
    public static readonly Dictionary<string, string[]> Map = new()
    {
        ["Normal"] = new[] { "Naya order", "Tayyari mein", "Tayyar hai", "Rawana", "Delivered" },
        ["Uniform"] = new[] { "Naya order", "Cutting", "Silai", "Finishing", "Tayyar hai", "Delivered" }
    };
}