using System.Collections.Generic;

public class BaseFlexController
{
    public List<FlexController> FlexControllers { get; private set; }

    public virtual void InitializeFlexControllers()
    {
        FlexControllers = new List<FlexController>
        {
            new FlexController { Name = "lid_raiser", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "lid_tightener", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "lid_droop", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "lid_closer", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "half_closed", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "blink", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "inner_raiser", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "outer_raiser", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "lowerer", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "cheek_raiser", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "wrinkler", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "dilator", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "upper_raiser", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "corner_puller", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "corner_depressor", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "chin_raiser", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "part", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "puckerer", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "funneler", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "stretcher", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "bite", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "presser", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "tightener", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "jaw_clencher", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "jaw_drop", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "mouth_drop", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "smile", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "lower_lip", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "head_rightleft", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "head_updown", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "eyes_updown", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "eyes_rightleft", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "body_rightleft", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "chest_rightleft", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "head_forwardbackward", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "gesture_updown", Min = 0f, Max = 1f, Default = 0f, Current = 0f },
            new FlexController { Name = "gesture_rightleft", Min = 0f, Max = 1f, Default = 0f, Current = 0f }
        };
    }
}
