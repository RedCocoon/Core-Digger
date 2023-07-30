public class Dirt : Cube
{
    public override void Dig(ToolType toolType)
    {
        base.Dig(toolType);
        switch (toolType)
        {
            case ToolType.Pickaxe:
                MinusHp(5);
                break;
            case ToolType.Shovel:
            case ToolType.Omnidrill:
                MinusHp(hp);
                break;
            case ToolType.Hand:
                MinusHp(1);
                break;
        }
    }
}
