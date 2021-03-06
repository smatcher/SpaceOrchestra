using UnityEngine;
using System.Collections;

public class Ship : MonoBehaviour {
	
	/* Private */
	private float pitch = 0;
	private float roll = 0;
	private float yaw = 0;
	private float desired_speed = 0;
	
	private Material shield_material;
	private float shield_timer;
	private bool shield_activated;
	
	
	/* Public */
	public bool is_controled;
	
	public float current_hull;
	public float current_energy;
	public float max_speed = 27.0f;
	public float min_speed = -1.0f;
	public float max_energy = 100.0f;
	public float max_hull = 100.0f;
	public float eps = 10.0f; /* Energy per seconds */
	
	public GameObject explosion;
	public AudioSource engine;
	
	public Weapon[] weapon_slots;
	
	// Use this for initialization
	void Start () {
		Transform shield = transform.Find("Shield");
		if(shield != null) {
			MeshRenderer shield_renderer = shield.gameObject.GetComponent<MeshRenderer>();
			shield_material = shield_renderer.material;
		} else {
			shield_material = null;
		}
		shield_activated = false;
		shield_timer = 0;

		if(shield_material != null) {
			Color col = Color.white;
			col.a = 0;
			shield_material.SetColor("_TintColor", col);
		}
		
		current_hull = max_hull;
		current_energy = max_energy;
	}
	
	public void setDesiredSpeed(float speed) {
		desired_speed = speed;
		if(desired_speed > max_speed)
			desired_speed = max_speed;
		if(desired_speed < min_speed)
			desired_speed = min_speed;
	}
	
	public void setPitch(float pitch) {
		this.pitch = pitch;
	}
	
	public void setRoll(float roll) {
		this.roll = roll;
	}
	
	public void setYaw(float yaw) {
		this.yaw = yaw;
	}
	
	public void Fire(int slot_nb) {
		if(weapon_slots[slot_nb].CanFire()) {
			weapon_slots[slot_nb].FireOneShot();
		}
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		
		/* Récupération de l'énergie */
		current_energy += eps * Time.deltaTime;
		if(current_energy > max_energy) {
			current_energy = max_energy;
		}

		rigidbody.AddForce(desired_speed * transform.forward);
		rigidbody.AddTorque(pitch * transform.right);
		rigidbody.AddTorque(roll * transform.forward);		                    
		rigidbody.AddTorque(yaw * transform.up);
		
		/* Reglage du pitch du son en fonction de la consigne vitesse */
		float current_speed = rigidbody.velocity.magnitude;
		engine.pitch = 1+ current_speed*(2/max_speed);
		
		
		if(shield_activated) {
			Color col = Color.white;			
			shield_timer -= Time.deltaTime;
			
			if(shield_timer > 0) {
				if(shield_timer > 0.7f) {
					col.a = 10 * (1-shield_timer);
				} else {
					col.a = shield_timer / 0.7f;
				}
			} else {
				shield_timer = 0;
				shield_activated = false;
				col.a = 0;
			}
			shield_material.SetColor("_TintColor", col);
		}
	}
		
	void OnImpact(int damage) {
		current_energy -= (float)damage;
		
		if(current_energy <= 0.0f) {
			current_hull += current_energy;
			current_energy = 0.0f;
		} else {
			shield_activated = true;
			shield_timer = 1;
		}
		if(current_hull <= 0.0f) {	
	    	Instantiate(explosion, transform.position, transform.rotation);
			Destroy(gameObject);
		}
	}	
		
	void OnCollisionEnter(Collision col) {
		OnImpact((int)(col.impactForceSum.magnitude * 10));
	}
}