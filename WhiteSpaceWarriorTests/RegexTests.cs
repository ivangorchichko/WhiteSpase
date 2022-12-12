using NUnit.Framework;
using WhiteSpaceWarrior;

namespace WhiteSpaceWarriorTests
{
    public class RegexTests
    {
        static string Compress(string content)
        {
            var compressed = new CSharpCompressors(new Options()
            {
                RemoveRegions = true,
                RemoveTags = new string[] { "revision" },
                RemoveParamNameUptoNWords = 2,
                RemoveSummaryUptoNWords = 2,
            }).Compress(content);
            return compressed.Trim();
        }

        /// <summary>
        /// </summary>
        public class Calculator
        {

            #region Properties
            /// <summary>
            /// Usage count
            /// </summary>
            private int CalculationCount
            {
                get;
                set;
            }
            #endregion

            #region Methods
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name="T"></typeparam>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            public int Add<T>(int a, int b)
            {

                CalculationCount++;
                return a + b;

            }

            ///////////////////////////////////////////////

            /// <summary>
            /// Minus impl
            /// </summary>
            /// <param name="a">A number</param>
            /// <param name="b">A number</param>
            /// <returns></returns>
            public int Minus(int a, int b)
            {

                CalculationCount++;
                return a + b;

            }

            #endregion


        }


        [Test]
        public void Showcase_all_features()
        {
            var code = @"
        /// <summary>
        /// </summary>
        public class Calculator
        {

            #region Properties
            /// <summary>
            /// Usage count
            /// </summary>
            private int CalculationCount
            {
                get;
                set;
            }
            #endregion

            #region Methods
            /// <summary>
            /// 
            /// </summary>
            /// <typeparam name=""T""></typeparam>
            /// <param name=""a""></param>
            /// <param name=""b""></param>
            /// <returns></returns>
            public int Add<T>(int a, int b)
            {

                CalculationCount++;
                return a + b;

            }

            ///////////////////////////////////////////////

            /// <summary>
            /// Minus impl
            /// </summary>
            /// <param name=""a"">A number</param>
            /// <param name=""b"">A number</param>
            /// <returns></returns>
            public int Minus(int a, int b)
            {

                CalculationCount++;
                return a - b;
            }

            #endregion


        }";

            var compressed = Compress(code);
            Assert.AreEqual(@"public class Calculator
        {
            private int CalculationCount { get; set; }

            public int Add<T>(int a, int b)
            {
                CalculationCount++;
                return a + b;
            }

            public int Minus(int a, int b)
            {
                CalculationCount++;
                return a - b;
            }
        }", compressed);
        }



        [Test]
        public void Summary_0_space()
        {
            var code = @"
                /// <summary>
                /// </summary>
                public static string CompressProperties(string file) {";

            Assert.AreEqual("public static string CompressProperties(string file) {", Compress(code));
        }

        [Test]
        public void Remove_empty_line_after_curly_start()
        {
            var code = @"
                {

                    int i;";

            Assert.AreEqual(@"{
                    int i;", Compress(code));
        }

        [Test]
        public void Remove_empty_lines_after_curly_start()
        {
            var code = @"
                {



                    int i;";

            Assert.AreEqual(@"{
                    int i;", Compress(code));
        }

        [Test]
        public void Remove_empty_lines_before_curlyend()
        {
            var code = @"
                ;



            }";

            Assert.AreEqual(@";
            }", Compress(code));
        }

        [Test]
        public void Remove_empty_line_before_curlyend()
        {
            var code = @"
                ;

            }";

            Assert.AreEqual(@";
            }", Compress(code));
        }

        [Test]
        public void Do_not_remove_empty_line_after_curlyend_when_next_char_is_not_curlyend()
        {
            var code = @"
                }


            int i;";

            Assert.AreEqual(@"}


            int i;", Compress(code));
        }


        [Test]
        public void Summary_1_space()
        {
            var code = @"
                /// <summary>
                /// 
                /// </summary>
                public static string CompressProperties(string file) {";

            Assert.AreEqual("public static string CompressProperties(string file) {", Compress(code));
        }

        [Test]
        public void Summary_mult_space()
        {
            var code = @"
                /// <summary>
                /// 
                /// 
                /// </summary>
                public static string CompressProperties(string file) {";

            Assert.AreEqual("public static string CompressProperties(string file) {", Compress(code));
        }

        [Test]
        public void Summary_multiline_comment_are_ignored()
        {
            var code = @"
                /// <summary>
                /// foo boo 
                /// bar baz
                /// </summary>
                public static string CompressProperties(string file) {";

            Assert.AreEqual(@"/// <summary>
                /// foo boo 
                /// bar baz
                /// </summary>
                public static string CompressProperties(string file) {", Compress(code));
        }

        [Test]
        public void Summary_singleline_comment_is_inlined()
        {
            var code = @"
                /// <summary>
                /// foo boo baz
                /// </summary>
                public static string CompressProperties(string file) {";

            Assert.AreEqual(@"/// <summary> foo boo baz </summary>
                public static string CompressProperties(string file) {", Compress(code));
        }

        [Test]
        public void PropertyCompress_get_set()
        {
            var code = @"
string MyProperty
{
    set;
    get;
}";

            Assert.AreEqual("string MyProperty { get; set; }", Compress(code));
        }

        [Test]
        public void PropertyCompress_set_get()
        {
            var code = @"
string MyProperty
{
    get;
    set;
}";
            Assert.AreEqual("string MyProperty { get; set; }", Compress(code));
        }


        [Test]
        public void OldStyleSeparator()
        {
            var code = @"        }

        //////////////////////////////////////////////////

        public void foo()";

            Assert.AreEqual(@"}

        public void foo()", Compress(code));

        }


        [Test]
        public void OldStyleSeparator_with_region()
        {
            var code = @"
        #region GetName
        /////////////////////////////////////////////////////////////////////////////////

        static foo";

            Assert.AreEqual(@"static foo", Compress(code));

        }


        [Test]
        public void OldStyleSeparator_with_if()
        {
            var code = @"}

        #if DEBUG
        /////////////////////////////////////////////////////////////////////////////////

        static foo";

            Assert.AreEqual(@"}

        #if DEBUG

        static foo", Compress(code));

        }

        [Test]
        public void OldStyleSeparator_with_endregion()
        {
            var code = @"}

        /////////////////////////////////////////////////////////////////////////////////
#endregion

        static foo";

            Assert.AreEqual(@"}


        static foo", Compress(code));

        }

        [Test]
        public void OldStyleSeparator_spaced_lines_are_not_matched()
        {
            var code = @"        }
                     
        //////////////////////////////////////////////////
                                
        public void foo()";

            Assert.AreEqual(code.Trim(), Compress(code));
        }


        [Test]
        public void Regions_are_removed_when_endregion_is_last_part_of_the_file()
        {
            var code = @"
#region private variables
    int i;
#endregion";

            Assert.AreEqual("int i;", Compress(code)); ;
        }

        [Test]
        public void Regions_are_removed_when_endregion_is_somewhere_in_the_file()
        {
            var code = @"
#region private variables
    int i;
#endregion
    int j;";

            Assert.AreEqual(@"int i;
    int j;", Compress(code)); ;
        }

        [Test]
        public void Remove_custom_tags_with_eager_matching()
        {
            var code = @"
            /// <revision version=""6.11.20"" date=""2015-09-19"" >
            /// implements nu interface 
            /// runs in singleinstance mode with dynamic recesions
            /// </revision>
            int i;
            /// do not match </revision>";

            Assert.AreEqual(@"int i;
            /// do not match </revision>", Compress(code)); ;
        }
    }
}