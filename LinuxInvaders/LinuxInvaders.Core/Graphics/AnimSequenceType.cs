namespace LinuxInvaders.Core.Graphics
{
    public enum AnimSequenceType
    {
        Default, //For when nothing applies
        Idle, MoveLeft, MoveRight, FlyingUp, FlyingDown, Pose, Hit, Attack, Death, //Characters
        RegularAttack, PowerAttack, ToxicAttack, EvilAttack, SpiritAttack, //Destructive animations
        Explosion, DustImpact, //Effects
        Creation //On creation, before regular animations
    }
}