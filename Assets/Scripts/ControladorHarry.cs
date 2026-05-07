using UnityEngine;
using System.Collections;

public class ControladorHarry : MonoBehaviour
{
    [Header("Componentes")]
    public Animator animator;
    public GameObject varita;
    private Transform camaraPrincipal;

    [Header("Ajustes de Velocidad")]
    public float velocidadCaminar = 5f;
    public float velocidadCorrer = 10f;
    public float velocidadRotacion = 10f;

    [Header("Ajustes de Combo (Clic Derecho)")]
    // Tiempo máximo entre clics para encadenar el combo (0.8s está bien para 2 golpes)
    public float tiempoMaximoCombo = 0.8f;
    private int pasoCombo = 0;
    private float tiempoUltimoGolpe = 0f;

    // Estado interno para saber si la varita está en la mano
    private bool tieneVarita = false;

    void Start()
    {
        camaraPrincipal = Camera.main.transform;
    }

    void Update()
    {
        // =========================================================
        // 1. LÓGICA DE MOVIMIENTO Y ANIMACIONES (W, A, S, D)
        // =========================================================

        // Leer las teclas W,A,S,D
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direccion = new Vector3(horizontal, 0f, vertical).normalized;

        // Si la dirección es mayor a 0.1, significa que estamos presionando una tecla
        bool seEstaMoviendo = direccion.magnitude >= 0.1f;

        // Saber si estamos presionando Shift
        bool corriendo = Input.GetKey(KeyCode.LeftShift);

        // ¡AQUÍ ESTÁ LA SOLUCIÓN! Le mandamos estas variables al Animator
        animator.SetBool("estaCaminando", seEstaMoviendo);
        animator.SetBool("estaCorriendo", corriendo);

        // Definir la velocidad final
        float velocidadActual = corriendo ? velocidadCorrer : velocidadCaminar;

        // Si nos estamos moviendo, aplicamos la rotación y el desplazamiento
        if (seEstaMoviendo)
        {
            float anguloDestino = Mathf.Atan2(direccion.x, direccion.z) * Mathf.Rad2Deg + camaraPrincipal.eulerAngles.y;
            Quaternion rotacion = Quaternion.Euler(0f, anguloDestino, 0f);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotacion, Time.deltaTime * velocidadRotacion);

            Vector3 direccionMovimiento = Quaternion.Euler(0f, anguloDestino, 0f) * Vector3.forward;
            transform.Translate(direccionMovimiento.normalized * velocidadActual * Time.deltaTime, Space.World);
        }

        // =========================================================
        // 2. SISTEMA DE EQUIPAR/DESEQUIPAR VARITA (TECLA 1)
        // =========================================================
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (!tieneVarita)
            {
                animator.SetTrigger("sacarVarita");
                StartCoroutine(CambiarEstadoVarita(true, 0.5f));
            }
            else
            {
                animator.SetTrigger("guardarVarita");
                StartCoroutine(CambiarEstadoVarita(false, 0.5f));
            }

            tieneVarita = !tieneVarita;
            animator.SetBool("tieneVarita", tieneVarita);
        }

        // =========================================================
        // 3. LÓGICA DE ATAQUE SIMPLE (CLIC IZQUIERDO)
        // =========================================================
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("golpear");
        }

        // =========================================================
        // 4. SISTEMA DE COMBOS (CLIC DERECHO) - 2 PASOS
        // =========================================================

        // Si ha pasado mucho tiempo desde el último clic, reiniciamos el combo a 0
        if (Time.time - tiempoUltimoGolpe > tiempoMaximoCombo)
        {
            pasoCombo = 0;
            animator.SetInteger("pasoCombo", pasoCombo);
        }

        // Detectar el clic derecho (botón 1 del ratón)
        if (Input.GetMouseButtonDown(1))
        {
            // Guardamos el momento exacto de este clic
            tiempoUltimoGolpe = Time.time;

            // Sumamos 1 al combo
            pasoCombo++;

            // Si nos pasamos de 2 (Kicking), volvemos a empezar el combo desde 1 (Hook Punch)
            if (pasoCombo > 2)
            {
                pasoCombo = 1;
            }

            // Le mandamos la información al Animator
            animator.SetInteger("pasoCombo", pasoCombo);
            animator.SetTrigger("ataqueCombo");
        }
    }

    // =========================================================
    // 5. CORRUTINA PARA SINCRONIZAR LA VARITA
    // =========================================================
    IEnumerator CambiarEstadoVarita(bool estado, float tiempoDeEspera)
    {
        yield return new WaitForSeconds(tiempoDeEspera);
        varita.SetActive(estado);
    }
}