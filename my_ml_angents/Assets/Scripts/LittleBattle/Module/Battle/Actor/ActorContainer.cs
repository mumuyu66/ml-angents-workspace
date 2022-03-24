using System;
using System.Collections.Generic;
using UnityEngine;

public class ActorContainer
{
	private static Dictionary<int, Actor> _actors = new Dictionary<int, Actor>();

	// 按照Layer对Actor进行分类，便于快速查找
	// ！！！！！！！！！ 仅包含进场的Actor
	private static List<Actor>[] _layer2actors = new List<Actor>[32];
	//不进场的在这里
	private static List<Actor>[] _layer2outActors = new List<Actor>[32];

	private static List<Actor> _all = new List<Actor>();


	static ActorContainer()
	{
		// 初始化 _layer2actors，避免每次访问都判空
		for (var i = 0; i < 32; i++)
		{
			_layer2actors[i] = new List<Actor>();
			_layer2outActors[i] = new List<Actor>();
		}
	}

	public static Actor Find(int uid)
	{
		Actor actor;
		if (_actors.TryGetValue(uid, out actor))
		{
			return actor;
		}
		return null;
	}

	public static Actor Find(Predicate<Actor> pred, IEnumerable<Actor> source)
	{
		if (source == null)
			source = _all;
		foreach (var n in source)
		{
			if (pred(n))
				return n;
		}
		return null;
	}

	public static Actor Find(Predicate<Actor> pred)
	{
		return Find(pred, null);
	}

	public static List<Actor> Filter()
	{
		return Filter(null, false, null);
	}

	public static List<Actor> Filter(Predicate<Actor> pred)
	{
		return Filter(pred, true, null);
	}

	public static List<Actor> Filter(bool sort)
	{
		return Filter(null, sort, null);
	}

	public static List<Actor> Filter(bool sort, Comparison<Actor> sortfun)
	{
		return Filter(null, sort, sortfun);
	}

	public static List<Actor> Filter(Predicate<Actor> pred, bool sort, Comparison<Actor> sortfun)
	{

		return Filter(_all, pred, sort, sortfun);
	}

	public static List<Actor> Filter(IEnumerable<Actor> source, Predicate<Actor> pred, bool sort, Comparison<Actor> sortfun)
	{
		List<Actor> l = ListPool<Actor>.Shared.Claim();
		if (pred == null)
		{
			// 原始list的副本，防止修改返回列表影响到原始list
			l.AddRange(source);
		}
		else
		{
			foreach (var n in source)
			{
				if (pred(n))
				{
					l.Add(n);
				}
			}
		}
		if (sort)
		{
			if (sortfun != null)
				l.Sort(sortfun);
			else
				l.Sort(defaultSort);
		}
		return l;
	}

	// 原地过滤，不生成新的List
	// ！！！！！！！！！ 注意：该方法会修改源list
	public static void InplaceFilter(List<Actor> source, Predicate<Actor> pred, Comparison<Actor> sortfun)
	{
		source.RemoveAll(actor => !pred(actor));
		if (sortfun != null)
		{
			source.Sort(sortfun);
		}
	}

	// 原地过滤，不生成新的List
	// ！！！！！！！！！ 注意：该方法会修改源list
	public static void InplaceFilter(List<Actor> source, Predicate<Actor> pred, bool sort)
	{
		InplaceFilter(source, pred, sort ? defaultSort : (Comparison<Actor>)null);
	}

	public static IReadOnlyList<Actor> allActors => _all;

	// 返回只读list，因为该list就是容器中的原始list，所以不可以修改
	public static IReadOnlyList<Actor> GetActorsInLayer(int layer, bool checkOutEntity = false)
	{
		if (!checkOutEntity)
			return _layer2actors[layer];
		else
		{
			List<Actor> ret = new List<Actor>();
			ret.AddRange(_layer2actors[layer]);
			ret.AddRange(_layer2outActors[layer]);
			return ret;
		}
	}

	public static List<Actor> FilterByLayerMask(IEnumerable<Actor> source, int layerMask)
	{
		return Filter(source, actor => (actor.Layer & layerMask) != 0, false, null);
	}

	public static List<Actor> FilterByLayerMask(int layerMask)
	{
		return FilterByLayerMask(_all, layerMask);
	}

	public static void InplaceFilterByLayerMask(List<Actor> source, int layerMask)
	{
		InplaceFilter(source, actor => (actor.Layer & layerMask) != 0, null);
	}


	public static void AddActor(Actor actor)
	{
		if (_actors.ContainsKey(actor.UUID))
		{
			Debug.LogWarning("AddActor event had actor where uid = " + actor.UUID);
			return;
		}
		_actors.Add(actor.UUID, actor);
		_all.Add(actor);

		if (actor.IsEntity)
			_layer2actors[actor.Layer].Add(actor);
		else
			_layer2outActors[actor.Layer].Add(actor);
	}

	public static Actor RemoveActor(int UUID)
	{
		Actor actor = null;
		if (_actors.TryGetValue(UUID, out actor))
		{
			_all.Remove(actor);
			_actors.Remove(UUID);

			if (actor.IsEntity && !_layer2actors[actor.Layer].Remove(actor))
			{
				Debug.LogError(string.Format("RemoveActor: miss actor in layer = {0} actorId = {1} , UUID = {2}", actor.Layer, actor.ActorId, actor.UUID));
				// 为了防止“泄露”，所有的Layer都移除一遍
				// 这个情况不应当发生，除非Actor中途意外改变了Layer
				for (var i = 0; i < 32; i++)
				{
					_layer2actors[i].Remove(actor);
				}
			}
			if (!actor.IsEntity && !_layer2outActors[actor.Layer].Remove(actor))
			{
				Debug.LogError(string.Format("RemoveActor: miss actor in layer = {0} actorId = {1} , UUID = {2}", actor.Layer, actor.ActorId, actor.UUID));
				for (var i = 0; i < 32; i++)
				{
					_layer2outActors[i].Remove(actor);
				}
			}
		}
		else
		{
			Debug.LogError(string.Format("RemoveActor: not found actor, UUID = {0}", UUID));
		}
		return actor;
	}

	public static void Cleanup()
	{
		foreach (var list in _layer2actors)
		{
			list.Clear();
		}
		foreach (var list in _layer2outActors)
		{
			list.Clear();
		}
		_all.Clear();
		_actors.Clear();
	}

	public static void SwitchActorLayer(int UUID, int oldLayer, int newLayer)
	{
		Actor actor = null;
		if (_actors.TryGetValue(UUID, out actor))
		{
			var actorSet = actor.IsEntity ? _layer2actors : _layer2outActors;
			if (!actorSet[oldLayer].Remove(actor))
			{
				Debug.LogError(string.Format("SwitchActorLayer: miss actor in layer = {0} actorId = {1} , UUID = {2}", actor.Layer, actor.ActorId, actor.UUID));
				// 为了防止“泄露”，所有的Layer都移除一遍
				// 这个情况不应当发生，除非Actor中途意外改变了Layer
				for (var i = 0; i < 32; i++)
				{
					actorSet[i].Remove(actor);
				}
			}
			actorSet[newLayer].Add(actor);
		}
		else
		{
			Debug.LogWarning("SwitchActorLayer  not find actor where uid = " + UUID);
		}
	}

	public static void SwitchActorIsEntity(int UUID, bool newIsEntity)
	{
		Actor actor = null;
		if (_actors.TryGetValue(UUID, out actor))
		{
			var layer = actor.Layer;
			var oldSet = newIsEntity ? _layer2outActors : _layer2actors;
			var newSet = newIsEntity ? _layer2actors : _layer2outActors;
			if (!oldSet[layer].Remove(actor))
			{
				Debug.LogError(string.Format("SwitchActorIsEntity: miss actor in layer = {0} actorId = {1} , UUID = {2}", actor.Layer, actor.ActorId, actor.UUID));
				// 为了防止“泄露”，所有的Layer都移除一遍
				// 这个情况不应当发生，除非Actor中途意外改变了Layer
				for (var i = 0; i < 32; i++)
				{
					oldSet[i].Remove(actor);
				}
			}
			newSet[layer].Add(actor);
		}
		else
		{
			Debug.LogWarning("SwitchActorIsEntity  not find actor where uid = " + UUID);
		}
	}

	private static int defaultSort(Actor a, Actor b)
	{
		return a.UUID - b.UUID;
	}

}

