﻿using System;

namespace Lunr.Globalization.ar
{
	public sealed class ArabicStopWordFilter : StopWordFilterBase
	{
		private const string Data =
			@"، اض امين اه اها اي ا اب اجل اجمع اخ اخذ اصبح اضحى اقبل اقل اكثر الا ام اما امامك امامك امسى اما ان انا انت انتم انتما انتن انت انشا انى او اوشك اولئك اولئكم اولاء اولالك اوه اي ايا اين اينما اي ان اي اف اذ اذا اذا اذما اذن الى اليكم اليكما اليكن اليك اليك الا اما ان انما اي اياك اياكم اياكما اياكن ايانا اياه اياها اياهم اياهما اياهن اياي ايه ان ا ابتدا اثر اجل احد اخرى اخلولق اذا اربعة ارتد استحال اطار اعادة اعلنت اف اكثر اكد الالاء الالى الا الاخيرة الان الاول الاولى التى التي الثاني الثانية الذاتي الذى الذي الذين السابق الف اللائي اللاتي اللتان اللتيا اللتين اللذان اللذين اللواتي الماضي المقبل الوقت الى اليوم اما امام امس ان انبرى انقلب انه انها او اول اي ايار ايام ايضا ب بات باسم بان بخ برس بسبب بس بشكل بضع بطان بعد بعض بك بكم بكما بكن بل بلى بما بماذا بمن بن بنا به بها بي بيد بين بس بله بئس تان تانك تبدل تجاه تحول تلقاء تلك تلكم تلكما تم تينك تين ته تي ثلاثة ثم ثم ثمة ثم جعل جلل جميع جير حار حاشا حاليا حاي حتى حرى حسب حم حوالى حول حيث حيثما حين حي حبذا حتى حذار خلا خلال دون دونك ذا ذات ذاك ذانك ذان ذلك ذلكم ذلكما ذلكن ذو ذوا ذواتا ذواتي ذيت ذينك ذين ذه ذي راح رجع رويدك ريث رب زيارة سبحان سرعان سنة سنوات سوف سوى ساء ساءما شبه شخصا شرع شتان صار صباح صفر صه صه ضد ضمن طاق طالما طفق طق ظل عاد عام عاما عامة عدا عدة عدد عدم عسى عشر عشرة علق على عليك عليه عليها عل عن عند عندما عوض عين عدس عما غدا غير  ف فان فلان فو فى في فيم فيما فيه فيها قال قام قبل قد قط قلما قوة كانما كاين كاي كاين كاد كان كانت كذا كذلك كرب كل كلا كلاهما كلتا كلم كليكما كليهما كلما كلا كم كما كي كيت كيف كيفما كان كخ لئن لا لات لاسيما لدن لدى لعمر لقاء لك لكم لكما لكن لكنما لكي لكيلا للامم لم لما لما لن لنا له لها لو لوكالة لولا لوما لي لست لست لستم لستما لستن لست لسن لعل لكن ليت ليس ليسا ليستا ليست ليسوا لسنا ما ماانفك مابرح مادام ماذا مازال مافتئ مايو متى مثل مذ مساء مع معاذ مقابل مكانكم مكانكما مكانكن مكانك مليار مليون مما ممن من منذ منها مه مهما من من نحن نحو نعم نفس نفسه نهاية نخ نعما نعم ها هاؤم هاك هاهنا هب هذا هذه هكذا هل هلم هلا هم هما هن هنا هناك هنالك هو هي هيا هيت هيا هؤلاء هاتان هاتين هاته هاتي هج هذا هذان هذين هذه هذي هيهات و وا واحد واضاف واضافت واكد وان واها واوضح وراءك وفي وقال وقالت وقد وقف وكان وكانت ولا ولم ومن وهو وهي ويكان وي وشكان يكون يمكن يوم ايان";

		private static readonly ISet<string> WordList =
			new Set<string>(Data.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries));

		protected override ISet<string> StopWords => WordList;
	}
}