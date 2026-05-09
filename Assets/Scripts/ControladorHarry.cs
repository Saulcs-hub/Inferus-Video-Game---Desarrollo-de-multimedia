using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class ControladorHarry : MonoBehaviour
{
    [Header("Componentes")]
    public Animator animator;
    public GameObject varita;
    private Transform camaraPrincipal;
    private Rigidbody rb;

    [Header("Ajustes de Velocidad y Movimiento")]
    public float velocidadCaminar = 5f;
    public float velocidadCorrer = 10f;
    public float velocidadAgachado = 2.5f;
    public float velocidadRotacion = 10f;
    public float fuerzaSalto = 6f;

    [Header("Ajustes de Combo (Clic Derecho)")]
    public float tiempoMaximoCombo = 0.8f;
    private int pasoCombo = 0;
    private float tiempoUltimoGolpe = 0f;

    // Estados
    private bool tieneVarita = false;
    private bool seEstaMoviendo;
    private bool corriendo;
    private bool estaAgachado;
    private bool enElSuelo = true;

    // Variables para pasar la dirección del Update al FixedUpdate
    private Vector3 vectorMovimientoFisicas;
    private Quaternion rotacionDestinoFisicas;

    void Start()
    {
        if (Camera.main != null) camaraPrincipal = Camera.main.transform;
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // 1. LEER INPUTS BÁSICOS
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direccion = new Vector3(horizontal, 0f, vertical).normalized;

        seEstaMoviendo = direccion.magnitude >= 0.1f;
        corriendo = Input.GetKey(KeyCode.LeftShift) && !estaAgachado;

        // 2. LEER INPUTS DE SALTO Y AGACHARSE
        if (Input.GetKeyDown(KeyCode.C))
        {
            estaAgachado = !estaAgachado;
        }

        if (Input.GetKeyDown(KeyCode.Space) && enElSuelo && !estaAgachado)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
            animator.SetTrigger("saltar");
            enElSuelo = false;
        }

        // 3. CALCULAR DIRECCIÓN Y ROTACIÓN
        if (seEstaMoviendo && camaraPrincipal != null)
        {
            float anguloDestino = Mathf.Atan2(direccion.x, direccion.z) * Mathf.Rad2Deg + camaraPrincipal.eulerAngles.y;
            rotacionDestinoFisicas = Quaternion.Euler(0f, anguloDestino, 0f);
            vectorMovimientoFisicas = rotacionDestinoFisicas * Vector3.forward;
        }

        // 4. ACTUALIZAR ANIMACIONES BÁSICAS
        animator.SetBool("estaCaminando", seEstaMoviendo);
        animator.SetBool("estaCorriendo", corriendo);
        animator.SetBool("estaAgachado", estaAgachado);

        // 5. ACCIONES DE COMBATE Y VARITA
        ManejarAcciones();
    }

    void FixedUpdate()
    {
        if (seEstaMoviendo)
        {
            float velocidadActual = estaAgachado ? velocidadAgachado : (corriendo ? velocidadCorrer : velocidadCaminar);
            rb.MoveRotation(Quaternion.Lerp(rb.rotation, rotacionDestinoFisicas, Time.fixedDeltaTime * velocidadRotacion));
            Vector3 movimiento = vectorMovimientoFisicas.normalized * velocidadActual * Time.fixedDeltaTime;
            movimiento.y = rb.linearVelocity.y * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movimiento);
        }
    }

    private void ManejarAcciones()
    {
        // --- SISTEMA DE COMBO REHECHO (Clic Derecho) ---
        if (Input.GetMouseButtonDown(1))
        {
            // Si pasó mucho tiempo desde el último golpe, reiniciamos a 1
            if (Time.time - tiempoUltimoGolpe > tiempoMaximoCombo)
            {
                pasoCombo = 1;
            }
            else
            {
                // Avanzamos el paso: de 1 a 2, y de 2 vuelve a 1
                pasoCombo = (pasoCombo >= 2) ? 1 : pasoCombo + 1;
            }

            tiempoUltimoGolpe = Time.time;
            animator.SetInteger("pasoCombo", pasoCombo);
            animator.SetTrigger("ataqueCombo");
        }

        // Resetear el parámetro del animator si el jugador se queda quieto mucho tiempo
        if (pasoCombo != 0 && (Time.time - tiempoUltimoGolpe > tiempoMaximoCombo))
        {
            pasoCombo = 0;
            animator.SetInteger("pasoCombo", pasoCombo);
        }

        // --- SISTEMA DE VARITA Y OTROS ---
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            animator.SetTrigger(tieneVarita ? "guardarVarita" : "sacarVarita");
            StartCoroutine(CambiarEstadoVarita(!tieneVarita, 0.5f));
            tieneVarita = !tieneVarita;
            animator.SetBool("tieneVarita", tieneVarita);
        }

        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("golpear");
        }
    }

    IEnumerator CambiarEstadoVarita(bool estado, float tiempoDeEspera)
    {
        yield return new WaitForSeconds(tiempoDeEspera);
        if (varita != null) varita.SetActive(estado);
    }

    private void OnCollisionStay(Collision collision) { enElSuelo = true; }
    private void OnCollisionExit(Collision collision) { enElSuelo = false; }
}