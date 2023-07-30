public class Tnt : Cube
{
    public override void Dig(ToolType toolType)
    {
        base.Dig(toolType);
        switch (toolType)
        {
            case ToolType.Pickaxe:
            case ToolType.Shovel:
            case ToolType.Hand:
            case ToolType.Omnidrill:
                MinusHp(hp);
                break;
        }
    }
}
