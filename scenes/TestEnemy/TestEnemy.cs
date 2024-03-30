using Godot;
using System;

public partial class TestEnemy : CharacterBody3D
{	
	enum STATE {
		IDLE,
		FOLLOW,
		HURT
	}
	
	[Export]
	public int number;

	STATE state;
	Player player;
	GpuParticles3D particle;
	MeshInstance3D dream;
	AnimationPlayer animPlayer;
	Timer hurtTimer;
	Area3D followArea;
	
	public override void _Ready()
	{
		state = STATE.FOLLOW;
		hurtTimer = GetNode<Timer>("HurtTimer");
		player = GetParent().GetNode<Player>("Player");
		animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
		particle = GetNode<GpuParticles3D>("GPUParticles3D");
		followArea = GetNode<Area3D>("Area3D");
		if(number == 0)
			dream = GetNode<MeshInstance3D>("Photo1");
		if(number == 1)
			dream = GetNode<MeshInstance3D>("Photo2");
		if(number == 2)
			dream = GetNode<MeshInstance3D>("Photo3");
		GD.Print(player == null);
	}
	
	public override void _PhysicsProcess(double delta)
	{
		switch (this.state) 
		{
			case STATE.IDLE :
			{
				MoveAndSlide();
				break;	
			} 
			case STATE.FOLLOW :
			{
				followPlayer((float)delta);
				break;	
			}
			case STATE.HURT :
			{
				checkIfHurt();
				break;
			}
		}
	}
	
		
	public int damage(int sentNumber, int count)
	{
		this.state = STATE.HURT;
		this.hurtTimer.WaitTime = 1.5f;
		this.hurtTimer.OneShot = true;
		this.hurtTimer.Start();
		followArea.Monitoring = false;
		GD.Print("pozovi");
		if(sentNumber == number && count > 0)
		{
			particle.Emitting = true;
			//animPlayer.Play("death");
			return number;
		}
		
		return -1;
	}

	public void show()
	{
		dream.Visible = true;
		GD.Print("pokazi");
	}

	public void unshow()
	{
		dream.Visible = false;
		GD.Print("unpokazi");
	}
	
	private void followPlayer(float delta)
	{
		Vector3 playerPos = player.GlobalPosition;
		this.LookAt(
			new Vector3(playerPos.X, this.GlobalPosition.Y, playerPos.Z),
			new Vector3(0.0f, 1.0f, 0.0f),
			false
		);
		Vector3 newPos = this
						.GlobalPosition
						.MoveToward(player.GlobalPosition, (float)delta);
		this.GlobalPosition = newPos;
	}

	// ANDREJ LEPO PULLUJ
	public void _on_attack_area_body_entered(Node3D body)
	{
		if (body is Player)
		{
			Player player = (Player)body;
			player.damage(30);
		}
	}
	
	public void _on_timer_timeout()
	{
		if (this.state == STATE.IDLE)
		{
			Random rnd = new Random();
			float randDeg = (float)rnd.NextDouble()*360.0f;
			float randRad = randDeg * (float)Math.PI / 180.0f;
			this.RotateY(randRad);		
			this.Velocity = new Vector3(1.0f, 0.0f, 0.0f);
			//this.Velocity = this.Velocity.Rotated(new Vector3(0.0f, 1.0f, 0.0f), randRad);
		}
	}
	
	public void checkIfHurt()
	{
		if (this.hurtTimer.TimeLeft <= 0.1f) 
		{
			this.state = STATE.IDLE;
			followArea.Monitoring = true;
		}
	}
	
	public void _on_area_3d_body_entered(Node3D body)
	{
		if(this.state != STATE.HURT) this.state = STATE.FOLLOW;
	}
	
	public void _on_area_3d_body_exited(Node3D body)
	{
		if(this.state != STATE.HURT) this.state = STATE.IDLE;
	}

	//public void _on_animation_player_animation_finished(String animName)
	//{
		//if(animName == "death")
			//QueueFree();
	//}
}
