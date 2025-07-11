using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectPlay : MonoBehaviour
{
    private ParticleSystem _particle;
    private float _duration;
    private WaitForSeconds _particleDuration;

    public GameObject ParticlePrfab;

    public Stack<GameObject> ParticlePool = new Stack<GameObject>();

    public static EffectPlay Intacne;

    private void Start()
    {
          CreatParticel();
    }

    private void Awake()
    {
        _particle = GetComponent<ParticleSystem>();
        _duration = _particle.main.duration;
        _particleDuration = new WaitForSeconds(_duration);
        if (Intacne == null)
        {
            Intacne = this;
        }
    }

    public void SetPositionAndPlay(Vector3 position)
    {
        transform.position = position;
        _particle.Play();
        StartCoroutine(DelayAndGotoPoolCoroutine());
    }

    private IEnumerator DelayAndGotoPoolCoroutine()
    {
        yield return _particleDuration;
        
    }

    public void CreatParticel()
    {
        for (int i = 0; i < 10; i++)
        {
           GameObject Particle = Instantiate(ParticlePrfab);
            ParticlePool.Push(Particle);
            Particle.SetActive(false);
        }
    }

    public void ResetItem()
    {
        _particle.Stop();
        _particle.Simulate(0); // 파티클 재생위치를 처음으로 되돌림
    }
}
