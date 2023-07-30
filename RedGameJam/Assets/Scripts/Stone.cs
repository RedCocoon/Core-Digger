public class Stone : Cube
{
    public override void Dig(ToolType toolType)
    {
        base.Dig(toolType);
        switch (toolType)
        {
            case ToolType.Pickaxe:
            case ToolType.Omnidrill:
                MinusHp(hp);
                break;
            case ToolType.Shovel:
                MinusHp(5);
                break;
            case ToolType.Hand:
                MinusHp(1);
                break;
        }
    }
}
