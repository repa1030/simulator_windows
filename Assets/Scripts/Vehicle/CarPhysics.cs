using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPhysics : MonoBehaviour
{
    [Header("Physics Configuration --------------")]
    public float downforce = 100.0f;
    public float drag = 0.22f;
    public float rollResistFactor = 0.014f;

    private float m_A_x_cW;
    private float m_rhoAir;
    private float m_airResist;
    private float m_gravity;
    private float m_fR;
    private float m_rollResist;
    private Vector3 m_velocity;
    // Start is called before the first frame update
    void Start()
    {
        m_rhoAir = 1.3f;
        m_A_x_cW = drag;
        m_airResist = 0.0f;
        m_fR = rollResistFactor;
        m_gravity = 9.81f;
        m_rollResist = 0.0f;
        m_velocity = Vector3.zero;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        m_velocity = this.transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);
        AddDownForce();
        AddDriveResist();
    }

    private void AddDriveResist()
    {
        m_A_x_cW = drag;
        m_fR = rollResistFactor;
        m_airResist = 0.5f * m_rhoAir * m_A_x_cW * m_velocity.z * m_velocity.z * Mathf.Sign(m_velocity.z);
        if (m_velocity.z <= -0.1 || m_velocity.z >= 0.1)
        {
            m_rollResist = this.GetComponent<Rigidbody>().mass * m_fR * m_gravity * Mathf.Sign(m_velocity.z);
        }
        else
        {
            m_rollResist = 0;
        }
        this.GetComponent<Rigidbody>().AddForce(-transform.forward * (m_airResist + m_rollResist));
    }

    private void AddDownForce()
    {
        this.GetComponent<Rigidbody>().AddForce(-transform.up * downforce * this.GetComponent<Rigidbody>().velocity.magnitude);
    }
}
